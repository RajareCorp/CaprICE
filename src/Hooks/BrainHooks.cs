using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using CaprICE.Security;
using CaprICE.Services;
using CaprICE.UI;

namespace CaprICE.Hooks
{
    /// <summary>Callbacks Harmony appliqués aux requêtes réseau et prompts de Capri.</summary>
    public static class BrainHooks
    {
        [ThreadStatic] private static string outgoingPreview;
        [ThreadStatic] private static string dlpBlockReason;

        public static bool OnBrainCreatePrefix(string url, int timeout, ref HttpWebRequest __result)
        {
            if (!string.IsNullOrEmpty(dlpBlockReason))
            {
                string reason = dlpBlockReason;
                dlpBlockReason = null;
                throw new WebException("[CaprICE Security Guard] Outbound request blocked by DLP: " + reason);
            }

            if (!AuthorizeConnection(url, "HTTP CONNECT"))
                throw new WebException("[CaprICE Security Guard] Outbound connection blocked.");
            return true;
        }

        public static void OnBrainCreatePostfix(ref HttpWebRequest __result, string url, int timeout)
        {
            if (__result != null && !string.IsNullOrEmpty(CaprICEHook.CustomProxyUrl))
            {
                try { __result.Proxy = new WebProxy(CaprICEHook.CustomProxyUrl); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Unable to apply CaprICE proxy: " + ex.Message); }
            }
        }

        public static bool OnBrainAskPrefix(object __instance, ref string text, ref string __result, ref string followLine, out bool __state)
        {
            __state = false;
            if (string.IsNullOrEmpty(text)) return true;

            string localReply;
            if (string.Equals(AppLaunchActions.NormalizeCommand(text), "Performance", StringComparison.OrdinalIgnoreCase))
            {
                __result = LocalPerformanceMonitor.GetReport();
                followLine = string.Empty;
                __state = true;
                return false;
            }
            if (AppLaunchActions.TryExecuteExact(text, out localReply))
            {
                __result = localReply;
                followLine = string.Empty;
                __state = true;
                return false;
            }

            BrainReflection.ApplyCustomSystemPrompt(__instance, CaprICEHook.CustomSystemPrompt);
            string initialText = text;
            int turnCount;
            string contextMemoryText = BrainReflection.ExtractContextAndMemory(__instance, out turnCount);
            string contextForScan = contextMemoryText ?? string.Empty;

            List<string> violations;
            string reason;
            InterceptStatus status = SecurityPolicy.EvaluatePayload(ref text, out violations, out reason);
            List<string> contextViolations = DlpEngine.Scan(contextForScan);
            foreach (string violation in contextViolations)
                if (!violations.Contains(violation)) violations.Add(violation);

            if (contextViolations.Count > 0 && SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp)
            {
                contextMemoryText = DlpEngine.Redact(contextMemoryText);
                if (SecurityPolicy.MaskDlpInPayloads)
                {
                    status = InterceptStatus.DlpMasked;
                    reason = "Données sensibles du contexte masquées avant transmission";
                }
                else
                {
                    status = InterceptStatus.Blocked;
                    reason = "Bloqué par Politique DLP (donnée sensible dans le contexte)";
                }
            }

            string targetUrl = BrainReflection.GetTargetUrl(__instance);
            InterceptEvent ev = new InterceptEvent
            {
                Url = "Capri://Brain/Ask",
                TargetName = TargetDetector.DetectTarget(targetUrl),
                IsExternal = TargetDetector.IsUrlExternal(targetUrl),
                Method = "PROMPT+CTX",
                RequestPayload = SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp
                    ? DlpEngine.Redact(initialText) : initialText,
                ContextMemory = contextMemoryText,
                HistoryTurns = turnCount,
                Status = status,
                Reason = reason,
                DlpMatches = violations
            };

            if (status == InterceptStatus.Blocked)
            {
                Report(ev);
                text = "Ma requête a été bloquée par la politique de sécurité CaprICE (" + reason + ").";
                return false;
            }

            Report(ev);
            return true;
        }

        public static bool OnBrainThinkPrefix(object __instance, object[] __args)
        {
            if (__args == null || __args.Length < 2) return true;
            string targetUrl = BrainReflection.GetTargetUrl(__instance);
            if (!TargetDetector.IsUrlExternal(targetUrl)) return true;

            bool enforce = SecurityPolicy.CurrentMode == PolicyMode.EnforceDlp;
            if (enforce)
            {
                System.Collections.IList messages = __args[1] as System.Collections.IList;
                System.Collections.IList safeMessages = BrainReflection.CloneOutgoingMessages(__args[1]) as System.Collections.IList;
                if (messages != null && safeMessages != null && messages.Count == safeMessages.Count)
                    for (int i = 0; i < messages.Count; i++) messages[i] = safeMessages[i];
            }
            string parts = __args.Length > 4 ? __args[4] as string : null;
            List<string> partViolations = string.IsNullOrEmpty(parts)
                ? new List<string>() : DlpEngine.Scan(RemoveImageData(parts));

            List<string> violations;
            string preview = BrainReflection.PrepareOutgoingMessages(__args[1], enforce, out violations);
            foreach (string violation in partViolations)
                if (!violations.Contains(violation)) violations.Add(violation);
            if (!string.IsNullOrEmpty(parts))
                preview += "\r\n[Payload multimodal joint ; texte inspecté, images non analysées par OCR]";
            outgoingPreview = preview;
            if (violations.Count == 0) return true;

            InterceptStatus status = InterceptStatus.Allowed;
            string reason = "Données sensibles détectées dans le contexte sortant (mode surveillance)";
            if (enforce)
            {
                if (partViolations.Count > 0)
                {
                    status = InterceptStatus.Blocked;
                    reason = "Bloqué par Politique DLP (instruction multimodale non masquée pour préserver le format)";
                    dlpBlockReason = reason;
                }
                else if (SecurityPolicy.MaskDlpInPayloads)
                {
                    status = InterceptStatus.DlpMasked;
                    reason = "Données sensibles masquées dans le contexte complet avant transmission";
                }
                else
                {
                    status = InterceptStatus.Blocked;
                    reason = "Bloqué par Politique DLP (donnée sensible dans le contexte complet)";
                    dlpBlockReason = reason;
                }
            }

            Report(new InterceptEvent
            {
                Url = TargetDetector.SanitizeUrl(targetUrl),
                TargetName = TargetDetector.DetectTarget(targetUrl),
                IsExternal = true,
                Method = "DLP CONTEXT",
                ContextMemory = preview,
                Status = status,
                Reason = reason,
                DlpMatches = violations
            });
            return true;
        }

        private static string RemoveImageData(string parts)
        {
            string withoutSourceData = Regex.Replace(parts,
                "(\"data\"\\s*:\\s*\")[^\"]*(\")", "$1[IMAGE_DATA]$2",
                RegexOptions.IgnoreCase);
            return Regex.Replace(withoutSourceData,
                "(\"url\"\\s*:\\s*\")data:image/[^;\" ]+;base64,[^\"]*(\")",
                "$1[IMAGE_DATA]$2", RegexOptions.IgnoreCase);
        }

        public static Exception OnBrainThinkFinalizer(Exception __exception)
        {
            outgoingPreview = null;
            dlpBlockReason = null;
            return __exception;
        }

        public static bool AuthorizeConnection(string url, string operation)
        {
            string reason;
            InterceptStatus status = SecurityPolicy.EvaluateConnection(url, out reason);
            if (status == InterceptStatus.PendingApproval)
            {
                string content = "Cible : " + TargetDetector.DetectTarget(url)
                    + "\r\nURL : " + TargetDetector.SanitizeUrl(url)
                    + "\r\n\r\n--- CONTEXTE SORTANT ---\r\n"
                    + (string.IsNullOrEmpty(outgoingPreview) ? "(Aucun aperçu de prompt disponible)" : outgoingPreview);
                if (!CaprICEForm.PromptConfirmation("Connexion externe", content, new List<string>()))
                {
                    status = InterceptStatus.Blocked;
                    reason = "Refusé par l'utilisateur";
                }
                else
                {
                    status = InterceptStatus.Allowed;
                    reason = "Connexion approuvée manuellement";
                }
            }

            Report(new InterceptEvent
            {
                Url = TargetDetector.SanitizeUrl(url),
                TargetName = TargetDetector.DetectTarget(url),
                IsExternal = TargetDetector.IsUrlExternal(url),
                Method = operation,
                Status = status,
                Reason = reason
            });
            return status != InterceptStatus.Blocked;
        }

        public static void OnBrainAskPostfix(string __result, bool __state)
        {
            if (__state || string.IsNullOrEmpty(__result)) return;

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

        public static void OnBrainLogPrefix(string url, long took, string what)
        {
            if (string.IsNullOrEmpty(url)) return;
            bool isError = took < 0 && (what ?? "").IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;

            InterceptEvent ev = new InterceptEvent
            {
                Url = TargetDetector.SanitizeUrl(url),
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

        private static void Report(InterceptEvent ev)
        {
            CaprICEForm.NotifyEvent(ev);
            AuditLog.Write(ev);
        }

        public static void OnBrainGreetingPostfix(ref string __result)
        {
            if (!string.IsNullOrEmpty(__result))
                __result = "[I.C.E Initialized] " + __result;
        }
    }
}
