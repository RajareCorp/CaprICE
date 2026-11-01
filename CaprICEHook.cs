using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace CaprICE
{
    public static class CaprICEHook
    {
        public static Assembly CapriAssembly = null;
        public static string CustomProxyUrl = null;
        public static string CustomSystemPrompt = null;
        public static bool IsInitialized { get; private set; }

        public static void Initialize(Assembly capriAssembly)
        {
            if (IsInitialized) return;

            try
            {
                Harmony harmony = new Harmony("com.caprice.security.interceptor");

                Type brainType = capriAssembly.GetType("CapriDesktop.Brain");
                if (brainType != null)
                {

                    // --- HOOK: Brain.Persona (Property Getter) ---
                    PropertyInfo personaProp = brainType.GetProperty("Persona", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (personaProp != null && personaProp.GetMethod != null)
                    {
                        MethodInfo personaPostfix = typeof(CaprICEHook).GetMethod("OnGetPersonaPostfix", BindingFlags.Public | BindingFlags.Static);
                        harmony.Patch(personaProp.GetMethod, postfix: new HarmonyMethod(personaPostfix));
                    }

                    // 1. Hook Brain.Create(string url, int timeout)
                    MethodInfo createMethod = brainType.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static);
                    if (createMethod != null)
                    {
                        MethodInfo createPrefix = typeof(CaprICEHook).GetMethod("OnBrainCreatePrefix", BindingFlags.Public | BindingFlags.Static);
                        MethodInfo createPostfix = typeof(CaprICEHook).GetMethod("OnBrainCreatePostfix", BindingFlags.Public | BindingFlags.Static);
                        harmony.Patch(createMethod, prefix: new HarmonyMethod(createPrefix), postfix: new HarmonyMethod(createPostfix));
                    }

                    // 2. Hook Brain.Ask(string text, bool followUps, out string followLine)
                    MethodInfo askMethod = brainType.GetMethod("Ask", BindingFlags.NonPublic | BindingFlags.Instance, null,
                        new Type[] { typeof(string), typeof(bool), typeof(string).MakeByRefType() }, null);
                    if (askMethod != null)
                    {
                        MethodInfo askPrefix = typeof(CaprICEHook).GetMethod("OnBrainAskPrefix", BindingFlags.Public | BindingFlags.Static);
                        MethodInfo askPostfix = typeof(CaprICEHook).GetMethod("OnBrainAskPostfix", BindingFlags.Public | BindingFlags.Static);
                        harmony.Patch(askMethod, prefix: new HarmonyMethod(askPrefix), postfix: new HarmonyMethod(askPostfix));
                    }

                    // 3. Hook Brain.Log(string url, long took, string what)
                    MethodInfo logMethod = brainType.GetMethod("Log", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                    if (logMethod != null)
                    {
                        MethodInfo logPrefix = typeof(CaprICEHook).GetMethod("OnBrainLogPrefix", BindingFlags.Public | BindingFlags.Static);
                        harmony.Patch(logMethod, prefix: new HarmonyMethod(logPrefix));
                    }
                }

                // 4. Hook Voice if present
                Type voiceType = capriAssembly.GetType("CapriDesktop.Voice");
                if (voiceType != null)
                {
                    MethodInfo voiceOpen = voiceType.GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);
                    if (voiceOpen != null)
                    {
                        MethodInfo voiceOpenPostfix = typeof(CaprICEHook).GetMethod("OnVoiceOpenPostfix", BindingFlags.Public | BindingFlags.Static);
                        harmony.Patch(voiceOpen, postfix: new HarmonyMethod(voiceOpenPostfix));
                    }
                }

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("CaprICE Hook Warning: " + ex.Message, "CaprICE Init",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        // --- HOOK: Brain.Create ---
        public static bool OnBrainCreatePrefix(string url, int timeout, ref HttpWebRequest __result)
        {
            List<string> violations;
            string reason;
            string dummy = "";
            InterceptStatus status = SecurityPolicy.Evaluate(url, ref dummy, out violations, out reason);

            if (status == InterceptStatus.Blocked)
            {
                InterceptEvent ev = new InterceptEvent
                {
                    Url = url,
                    TargetName = SecurityPolicy.DetectTarget(url),
                    IsExternal = SecurityPolicy.IsUrlExternal(url),
                    Method = "HTTP CONNECT",
                    Status = InterceptStatus.Blocked,
                    Reason = reason
                };
                CaprICEForm.NotifyEvent(ev);
                SecurityPolicy.WriteAuditLog(ev);

                throw new WebException("[CaprICE Security Guard] Outbound connection blocked: " + reason);
            }

            return true;
        }

        public static void OnBrainCreatePostfix(ref HttpWebRequest __result, string url, int timeout)
        {
            if (__result != null && !string.IsNullOrEmpty(CustomProxyUrl))
            {
                try
                {
                    __result.Proxy = new WebProxy(CustomProxyUrl);
                }
                catch { }
            }
        }

        // --- HOOK: Brain.Ask (Prompt interception, DLP, Context & Memory extraction) ---
        public static bool ClearBrainHistory(object brainInstance = null)
        {
            try
            {
                if (brainInstance != null)
                {
                    FieldInfo histField = brainInstance.GetType().GetField("history", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (histField != null)
                    {
                        var histList = histField.GetValue(brainInstance) as IList;
                        if (histList != null)
                        {
                            histList.Clear();
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }


        public static bool OnBrainAskPrefix(object __instance, ref string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            // Injection / Mise à jour dynamique du message système dans l'historique actif
            if (!string.IsNullOrEmpty(CustomSystemPrompt) && __instance != null)
            {
                try
                {
                    FieldInfo histField = __instance.GetType().GetField("history", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (histField != null)
                    {
                        var history = histField.GetValue(__instance) as IList;
                        if (history != null)
                        {
                            bool systemFound = false;
                            foreach (var msg in history)
                            {
                                if (msg == null) continue;

                                var roleProp = msg.GetType().GetProperty("Role");
                                var contentProp = msg.GetType().GetProperty("Content");

                                string role = roleProp?.GetValue(msg, null) as string;
                                if (role == "system")
                                {
                                    contentProp?.SetValue(msg, CustomSystemPrompt, null);
                                    systemFound = true;
                                }
                            }

                            // Si la conversation a démarré sans message 'system' explicite, on l'insère au début
                            if (!systemFound && history.Count > 0)
                            {
                                Type msgType = history[0].GetType();
                                // On instancie un ChatMessage("system", CustomSystemPrompt) via Reflection si nécessaire
                            }
                        }
                    }
                }
                catch { }
            }

            string initialText = text;
            List<string> violations;
            string reason;

            // Extract Capri's Context & Long-Term Memory
            int turnCount;
            string contextMemoryText = ExtractContextAndMemory(__instance, out turnCount);

            string targetUrl = "https://api.cloud-ai.provider/v1/chat";
            InterceptStatus status = SecurityPolicy.Evaluate(targetUrl, ref text, out violations, out reason);

            // Also check if any sensitive data resides in the context/memory
            if (!string.IsNullOrEmpty(contextMemoryText))
            {
                List<string> memViolations;
                string memReason;
                string dummy = contextMemoryText;
                SecurityPolicy.Evaluate(targetUrl, ref dummy, out memViolations, out memReason);
                foreach (string v in memViolations)
                {
                    if (!violations.Contains(v)) violations.Add("En Mémoire : " + v);
                }
            }

            InterceptEvent ev = new InterceptEvent
            {
                Url = "Capri://Brain/Ask",
                TargetName = "Prompt & Contexte -> Brain",
                IsExternal = SecurityPolicy.CurrentMode == PolicyMode.BlockExternal,
                Method = "PROMPT+CTX",
                RequestPayload = initialText,
                ContextMemory = contextMemoryText,
                HistoryTurns = turnCount,
                Status = status,
                Reason = reason,
                DlpMatches = violations
            };

            if (status == InterceptStatus.DlpMasked)
            {
                ev.RequestPayload = text; // Sanitized text
                CaprICEForm.NotifyEvent(ev);
                SecurityPolicy.WriteAuditLog(ev);
                return true;
            }
            else if (status == InterceptStatus.Blocked)
            {
                CaprICEForm.NotifyEvent(ev);
                SecurityPolicy.WriteAuditLog(ev);
                text = "Ma requête a été bloquée par la politique de sécurité CaprICE (" + reason + ").";
                return false;
            }
            else if (status == InterceptStatus.PendingApproval)
            {
                string confirmDisplay = "PROMPT ACTUEL :\r\n" + initialText + "\r\n\r\n--- CONTEXTE / MÉMOIRE ENVOYÉ ---\r\n" + (contextMemoryText ?? "(Aucune mémoire additionnelle)");
                bool approved = CaprICEForm.PromptConfirmation("Transmission IA Externe", confirmDisplay, violations);
                if (!approved)
                {
                    ev.Status = InterceptStatus.Blocked;
                    ev.Reason = "Refusé par l'utilisateur";
                    CaprICEForm.NotifyEvent(ev);
                    SecurityPolicy.WriteAuditLog(ev);
                    text = "Requête refusée par l'utilisateur dans CaprICE.";
                    return false;
                }
                ev.Status = InterceptStatus.Allowed;
                ev.Reason = "Approuvé manuellement par l'utilisateur";
            }

            CaprICEForm.NotifyEvent(ev);
            SecurityPolicy.WriteAuditLog(ev);
            return true;
        }

        private static string ExtractContextAndMemory(object brainInstance, out int turnCount)
        {
            turnCount = 0;
            if (brainInstance == null) return null;

            StringBuilder sb = new StringBuilder();
            try
            {
                Type brainType = brainInstance.GetType();

                // 1. Long-term memory notes (me.md / memories.md)
                FieldInfo memField = brainType.GetField("MemoryPath", BindingFlags.NonPublic | BindingFlags.Static);
                string memPath = memField != null ? memField.GetValue(null) as string : null;
                if (string.IsNullOrEmpty(memPath) || !File.Exists(memPath))
                {
                    string localCapri = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri");
                    string candidateMem = Path.Combine(localCapri, "memories.md");
                    if (File.Exists(candidateMem)) memPath = candidateMem;
                    else
                    {
                        string candidateMe = Path.Combine(localCapri, "me.md");
                        if (File.Exists(candidateMe)) memPath = candidateMe;
                    }
                }

                if (!string.IsNullOrEmpty(memPath) && File.Exists(memPath))
                {
                    sb.AppendLine("=== MÉMOIRE PERSISTANTE CAPRI (" + Path.GetFileName(memPath) + ") ===");
                    sb.AppendLine(File.ReadAllText(memPath));
                    sb.AppendLine();
                }

                // 2. Conversation History Turns
                PropertyInfo histProp = brainType.GetProperty("History", BindingFlags.NonPublic | BindingFlags.Instance);
                if (histProp != null)
                {
                    IEnumerable histList = histProp.GetValue(brainInstance, null) as IEnumerable;
                    if (histList != null)
                    {
                        List<string> turns = new List<string>();
                        foreach (object item in histList)
                        {
                            if (item != null)
                            {
                                Type msgType = item.GetType();
                                PropertyInfo roleProp = msgType.GetProperty("Role");
                                PropertyInfo contentProp = msgType.GetProperty("Content");
                                string role = roleProp != null ? roleProp.GetValue(item, null) as string : "msg";
                                string content = contentProp != null ? contentProp.GetValue(item, null) as string : "";
                                turns.Add("[" + (role ?? "").ToUpperInvariant() + "] " + content);
                            }
                        }

                        turnCount = turns.Count;
                        if (turns.Count > 0)
                        {
                            sb.AppendLine("=== HISTORIQUE DE CONVERSATION ENVOYÉ (" + turns.Count + " tours) ===");
                            foreach (string t in turns) sb.AppendLine(t);
                        }
                    }
                }
            }
            catch { }

            return sb.Length > 0 ? sb.ToString() : "(Mémoire et historique vides pour l'instant)";
        }

        public static void OnBrainAskPostfix(string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                InterceptEvent ev = new InterceptEvent
                {
                    Url = "Capri://Brain/Reply",
                    TargetName = "Réponse IA -> Brain",
                    IsExternal = false,
                    Method = "REPLY",
                    ResponsePayload = __result,
                    Status = InterceptStatus.Allowed,
                    Reason = "Réponse reçue (" + __result.Length + " caractères)"
                };
                CaprICEForm.NotifyEvent(ev);
            }
        }

        // --- HOOK: Brain.Log ---
        public static void OnBrainLogPrefix(string url, long took, string what)
        {
            if (string.IsNullOrEmpty(url)) return;

            bool isError = took < 0 && (what ?? "").IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
            InterceptStatus status = isError ? InterceptStatus.Blocked : InterceptStatus.Allowed;

            InterceptEvent ev = new InterceptEvent
            {
                Url = url,
                TargetName = SecurityPolicy.DetectTarget(url),
                IsExternal = SecurityPolicy.IsUrlExternal(url),
                Method = "ENDPOINT",
                LatencyMs = took,
                Reason = what,
                ResponsePayload = what,
                Status = status
            };

            CaprICEForm.NotifyEvent(ev);
            SecurityPolicy.WriteAuditLog(ev);
        }

        // --- HOOK: Voice.Open ---
        public static void OnVoiceOpenPostfix(ref HttpWebRequest __result, string path, string key, string method)
        {
            if (__result != null && !string.IsNullOrEmpty(CustomProxyUrl))
            {
                try { __result.Proxy = new WebProxy(CustomProxyUrl); } catch { }
            }

            InterceptEvent ev = new InterceptEvent
            {
                Url = "https://api.elevenlabs.io" + path,
                TargetName = "ElevenLabs Voice",
                IsExternal = true,
                Method = method,
                Status = InterceptStatus.Allowed,
                Reason = "Session audio / TTS ouverte"
            };
            CaprICEForm.NotifyEvent(ev);
        }

        public static void OnGetPersonaPostfix(ref string __result)
        {
            if (!string.IsNullOrEmpty(CustomSystemPrompt))
            {
                // Remplacer complètement le prompt initial de Capri par le prompt personnalisé
                __result = CustomSystemPrompt;
            }
        }
    }
}
