using System;
using System.Collections.Generic;
using System.Net;
using CaprICE.Security;
using CaprICE.UI;

namespace CaprICE.Hooks
{
    /// <summary>Callbacks Harmony appliqués à CapriDesktop.Brain (Create, Ask, Log).</summary>
    public static class BrainHooks
    {
        // --- Brain.Create : blocage des connexions + proxy amont ---
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
                    TargetName = TargetDetector.DetectTarget(url),
                    IsExternal = TargetDetector.IsUrlExternal(url),
                    Method = "HTTP CONNECT",
                    Status = InterceptStatus.Blocked,
                    Reason = reason
                };
                Report(ev);

                throw new WebException("[CaprICE Security Guard] Outbound connection blocked: " + reason);
            }

            return true;
        }

        public static void OnBrainCreatePostfix(ref HttpWebRequest __result, string url, int timeout)
        {
            if (__result != null && !string.IsNullOrEmpty(CaprICEHook.CustomProxyUrl))
            {
                try
                {
                    __result.Proxy = new WebProxy(CaprICEHook.CustomProxyUrl);
                }
                catch { }
            }
        }

        // --- Brain.Ask : interception du prompt, DLP, contexte & mémoire ---
        public static bool OnBrainAskPrefix(object __instance, ref string text)
        {
            if (string.IsNullOrEmpty(text)) return true;

            BrainReflection.ApplyCustomSystemPrompt(__instance, CaprICEHook.CustomSystemPrompt);

            string initialText = text;
            List<string> violations;
            string reason;

            int turnCount;
            string contextMemoryText = BrainReflection.ExtractContextAndMemory(__instance, out turnCount);

            string targetUrl = "https://api.cloud-ai.provider/v1/chat";
            InterceptStatus status = SecurityPolicy.Evaluate(targetUrl, ref text, out violations, out reason);

            // Vérifie aussi les données sensibles présentes dans le contexte / la mémoire
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
                ev.RequestPayload = text; // texte assaini
                Report(ev);
                return true;
            }

            if (status == InterceptStatus.Blocked)
            {
                Report(ev);
                text = "Ma requête a été bloquée par la politique de sécurité CaprICE (" + reason + ").";
                return false;
            }

            if (status == InterceptStatus.PendingApproval)
            {
                string confirmDisplay = "PROMPT ACTUEL :\r\n" + initialText + "\r\n\r\n--- CONTEXTE / MÉMOIRE ENVOYÉ ---\r\n" + (contextMemoryText ?? "(Aucune mémoire additionnelle)");
                bool approved = CaprICEForm.PromptConfirmation("Transmission IA Externe", confirmDisplay, violations);
                if (!approved)
                {
                    ev.Status = InterceptStatus.Blocked;
                    ev.Reason = "Refusé par l'utilisateur";
                    Report(ev);
                    text = "Requête refusée par l'utilisateur dans CaprICE.";
                    return false;
                }
                ev.Status = InterceptStatus.Allowed;
                ev.Reason = "Approuvé manuellement par l'utilisateur";
            }

            Report(ev);
            return true;
        }

        public static void OnBrainAskPostfix(string __result)
        {
            if (string.IsNullOrEmpty(__result)) return;

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

        // --- Brain.Log : suivi des endpoints appelés ---
        public static void OnBrainLogPrefix(string url, long took, string what)
        {
            if (string.IsNullOrEmpty(url)) return;

            bool isError = took < 0 && (what ?? "").IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;

            InterceptEvent ev = new InterceptEvent
            {
                Url = url,
                TargetName = TargetDetector.DetectTarget(url),
                IsExternal = TargetDetector.IsUrlExternal(url),
                Method = "ENDPOINT",
                LatencyMs = took,
                Reason = what,
                ResponsePayload = what,
                Status = isError ? InterceptStatus.Blocked : InterceptStatus.Allowed
            };
            Report(ev);
        }

        /// <summary>Affiche l'événement dans l'interface et l'écrit dans le journal d'audit.</summary>
        private static void Report(InterceptEvent ev)
        {
            CaprICEForm.NotifyEvent(ev);
            AuditLog.Write(ev);
        }
        
        public static void OnBrainGreetingPostfix(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
            {
                __result = "I.C.E Initialized. " + __result;
            }
        }
    }
}
