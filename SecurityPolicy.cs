using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CaprICE
{
    public enum PolicyMode
    {
        MonitorOnly,
        EnforceDlp,
        BlockExternal,
        AskConfirmation
    }

    public enum InterceptStatus
    {
        Allowed,
        Blocked,
        DlpMasked,
        PendingApproval
    }

    public class InterceptEvent
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string TargetName { get; set; }
        public bool IsExternal { get; set; }
        public string Method { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string ContextMemory { get; set; }
        public int HistoryTurns { get; set; }
        public InterceptStatus Status { get; set; }
        public string Reason { get; set; }
        public long LatencyMs { get; set; }
        public List<string> DlpMatches { get; set; }

        public InterceptEvent()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            Timestamp = DateTime.Now;
            DlpMatches = new List<string>();
            Status = InterceptStatus.Allowed;
            LatencyMs = -1;
        }
    }

    public static class SecurityPolicy
    {
        public static PolicyMode CurrentMode = PolicyMode.MonitorOnly;
        public static bool MaskDlpInPayloads = true;

        // Toggles for individual DLP rules
        public static bool MaskApiKeys = true;
        public static bool MaskPasswords = true;
        public static bool MaskUserPaths = true;
        public static bool MaskEmails = true;
        public static bool MaskPhoneNumbers = true;
        public static bool MaskIpAddresses = true;
        public static bool MaskPostalAddresses = true;
        public static bool MaskCreditCards = true;
        public static bool MaskCustomNames = true;

        // User Custom Data to always redact
        public static string CustomNames = "";        // e.g. "Jean Dupont, Marie Curie"
        public static string CustomAddresses = "";    // e.g. "12 rue des Fleurs, 75001 Paris"

        private static string StorageDirectory
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri", "CaprICE");
                if (!Directory.Exists(dir))
                {
                    try { Directory.CreateDirectory(dir); } catch { return AppDomain.CurrentDomain.BaseDirectory; }
                }
                return dir;
            }
        }

        public static string AuditLogPath { get { return Path.Combine(StorageDirectory, "caprice_audit.jsonl"); } }
        public static string SettingsPath { get { return Path.Combine(StorageDirectory, "caprice_settings.json"); } }

        private static readonly object LogLock = new object();

        // Built-in Regex Patterns
        private static readonly Regex RxApiKeys = new Regex(@"\b(sk-[a-zA-Z0-9_\-]{20,}|anthropic-[a-zA-Z0-9_\-]{20,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxSecrets = new Regex(@"(api[_-]?key|access[_-]?token|bearer|secret)\s*[:=]\s*[""']?([a-zA-Z0-9_\-]{16,})[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxPasswords = new Regex(@"(password|passwd|pwd|motdepasse)\s*[:=]\s*[""']?([^""'\s]{4,})[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxUserPaths = new Regex(@"[A-Za-z]:\\Users\\[a-zA-Z0-9_.\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxEmails = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
        private static readonly Regex RxCreditCards = new Regex(@"\b(?:\d{4}[- ]?){3}\d{4}\b", RegexOptions.Compiled);

        // Phone Numbers: French (+33 / 06 / 07 / 01...) and International formats
        private static readonly Regex RxPhones = new Regex(@"(?:(?:\+|00)33|0)\s*[1-9](?:[\s.-]*\d{2}){4}|\b(?:\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled);

        // IP Addresses (IPv4 and standard IPv6)
        private static readonly Regex RxIpv4 = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.Compiled);
        private static readonly Regex RxIpv6 = new Regex(@"\b(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\b", RegexOptions.Compiled);

        // Postal Addresses: "12 rue de la Paix", "45 avenue Foch", "10 boulevard Saint-Michel", etc.
        private static readonly Regex RxPostalAddresses = new Regex(@"\b\d{1,5}(?:\s*(?:bis|ter))?\s+(?:rue|avenue|av\.|boulevard|bd|chemin|allée|impasse|route|place|quai|square|voie|st\.|street|road|rd\.|avenue|blvd)\s+[A-Za-zÀ-ÿ0-9'\-\s]{3,35}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RxPostalCodes = new Regex(@"\b(?:code\s*postal\s*[:=]?\s*|[A-Z]{2}-)?\d{5}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static SecurityPolicy()
        {
            LoadSettings();
        }

        public static bool IsUrlExternal(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return !url.Contains("127.0.0.1") && !url.Contains("localhost");
        }

        public static string DetectTarget(string url)
        {
            if (string.IsNullOrEmpty(url)) return "Inconnu";
            string lower = url.ToLowerInvariant();
            if (lower.Contains("11434") || lower.Contains("/api/chat") || lower.Contains("/api/tags")) return "Ollama (Local)";
            if (lower.Contains("127.0.0.1") || lower.Contains("localhost")) return "Serveur Local";
            if (lower.Contains("api.openai.com")) return "OpenAI (Cloud)";
            if (lower.Contains("api.anthropic.com")) return "Anthropic Claude (Cloud)";
            if (lower.Contains("deepseek.com")) return "DeepSeek (Cloud)";
            if (lower.Contains("googleapis.com")) return "Google Gemini (Cloud)";
            if (lower.Contains("nvidia.com")) return "Nvidia NIM (Cloud)";
            if (lower.Contains("openrouter.ai")) return "OpenRouter (Cloud)";
            if (lower.Contains("elevenlabs.io")) return "ElevenLabs Voice (Cloud)";
            if (lower.Contains("github.com")) return "GitHub";
            return "API Externe";
        }

        public static InterceptStatus Evaluate(string url, ref string payload, out List<string> violations, out string reason)
        {
            violations = new List<string>();
            reason = string.Empty;
            bool isExternal = IsUrlExternal(url);

            // 1. Strict Local Mode
            if (CurrentMode == PolicyMode.BlockExternal && isExternal)
            {
                reason = "Bloqué par Politique Strict Local (IA Cloud externe interdite)";
                return InterceptStatus.Blocked;
            }

            // 2. Scan DLP Rules
            if (!string.IsNullOrEmpty(payload))
            {
                CheckRule(RxApiKeys, "Clé API", payload, violations, MaskApiKeys);
                CheckRule(RxSecrets, "Token / Secret", payload, violations, MaskApiKeys);
                CheckRule(RxPasswords, "Mot de passe", payload, violations, MaskPasswords);
                CheckRule(RxUserPaths, "Chemin Utilisateur Windows", payload, violations, MaskUserPaths);
                CheckRule(RxEmails, "Adresse Email", payload, violations, MaskEmails);
                CheckRule(RxCreditCards, "Numéro Carte Bancaire", payload, violations, MaskCreditCards);
                CheckRule(RxPhones, "Numéro de Téléphone", payload, violations, MaskPhoneNumbers);
                CheckRule(RxIpv4, "Adresse IPv4", payload, violations, MaskIpAddresses);
                CheckRule(RxIpv6, "Adresse IPv6", payload, violations, MaskIpAddresses);
                CheckRule(RxPostalAddresses, "Adresse Postale", payload, violations, MaskPostalAddresses);
                CheckRule(RxPostalCodes, "Code Postal", payload, violations, MaskPostalAddresses);

                // Custom User Names & Surnames
                if (MaskCustomNames && !string.IsNullOrWhiteSpace(CustomNames))
                {
                    string[] names = CustomNames.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string rawName in names)
                    {
                        string name = rawName.Trim();
                        if (name.Length >= 2)
                        {
                            Regex rxName = new Regex(@"\b" + Regex.Escape(name) + @"\b", RegexOptions.IgnoreCase);
                            if (rxName.IsMatch(payload))
                            {
                                violations.Add("Nom / Prénom Identifié (" + name + ")");
                            }
                        }
                    }
                }

                // Custom User Addresses
                if (MaskPostalAddresses && !string.IsNullOrWhiteSpace(CustomAddresses))
                {
                    string[] addrs = CustomAddresses.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string rawAddr in addrs)
                    {
                        string addr = rawAddr.Trim();
                        if (addr.Length >= 3)
                        {
                            Regex rxAddr = new Regex(Regex.Escape(addr), RegexOptions.IgnoreCase);
                            if (rxAddr.IsMatch(payload))
                            {
                                violations.Add("Adresse Personnelle (" + addr + ")");
                            }
                        }
                    }
                }
            }

            // 3. Enforce DLP Rule
            if (violations.Count > 0)
            {
                if (CurrentMode == PolicyMode.EnforceDlp)
                {
                    if (MaskDlpInPayloads)
                    {
                        // Mask sensitive parts
                        if (MaskApiKeys) { payload = RxApiKeys.Replace(payload, "[REDACTED:API_KEY]"); payload = RxSecrets.Replace(payload, "$1: \"[REDACTED:TOKEN]\""); }
                        if (MaskPasswords) payload = RxPasswords.Replace(payload, "$1: \"[REDACTED:PASSWORD]\"");
                        if (MaskUserPaths) payload = RxUserPaths.Replace(payload, "C:\\Users\\[REDACTED_USER]");
                        if (MaskEmails) payload = RxEmails.Replace(payload, "[REDACTED:EMAIL]");
                        if (MaskCreditCards) payload = RxCreditCards.Replace(payload, "[REDACTED:CARD]");
                        if (MaskPhoneNumbers) payload = RxPhones.Replace(payload, "[REDACTED:PHONE]");
                        if (MaskIpAddresses) { payload = RxIpv4.Replace(payload, "[REDACTED:IP]"); payload = RxIpv6.Replace(payload, "[REDACTED:IPV6]"); }
                        if (MaskPostalAddresses) { payload = RxPostalAddresses.Replace(payload, "[REDACTED:ADDRESS]"); payload = RxPostalCodes.Replace(payload, "[REDACTED:ZIP]"); }

                        if (MaskCustomNames && !string.IsNullOrWhiteSpace(CustomNames))
                        {
                            string[] names = CustomNames.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rawName in names)
                            {
                                string name = rawName.Trim();
                                if (name.Length >= 2)
                                {
                                    Regex rxName = new Regex(@"\b" + Regex.Escape(name) + @"\b", RegexOptions.IgnoreCase);
                                    payload = rxName.Replace(payload, "[REDACTED:NOM]");
                                }
                            }
                        }

                        if (MaskPostalAddresses && !string.IsNullOrWhiteSpace(CustomAddresses))
                        {
                            string[] addrs = CustomAddresses.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string rawAddr in addrs)
                            {
                                string addr = rawAddr.Trim();
                                if (addr.Length >= 3)
                                {
                                    Regex rxAddr = new Regex(Regex.Escape(addr), RegexOptions.IgnoreCase);
                                    payload = rxAddr.Replace(payload, "[REDACTED:ADRESSE]");
                                }
                            }
                        }

                        reason = "Données sensibles masquées avant transmission (" + string.Join(", ", violations.ToArray()) + ")";
                        return InterceptStatus.DlpMasked;
                    }
                    else
                    {
                        reason = "Bloqué par Politique DLP (" + string.Join(", ", violations.ToArray()) + ")";
                        return InterceptStatus.Blocked;
                    }
                }
            }

            // 4. Manual confirmation required
            if (CurrentMode == PolicyMode.AskConfirmation && isExternal)
            {
                reason = "En attente d'approbation manuelle";
                return InterceptStatus.PendingApproval;
            }

            reason = "Autorisé";
            return InterceptStatus.Allowed;
        }

        private static void CheckRule(Regex rx, string name, string payload, List<string> violations, bool enabled)
        {
            if (!enabled) return;
            MatchCollection matches = rx.Matches(payload);
            if (matches.Count > 0)
            {
                violations.Add(name + " (" + matches.Count + " détection" + (matches.Count > 1 ? "s" : "") + ")");
            }
        }

        public static void WriteAuditLog(InterceptEvent ev)
        {
            try
            {
                lock (LogLock)
                {
                    string safePayload = (ev.RequestPayload ?? "").Replace("\r", " ").Replace("\n", " ");
                    if (safePayload.Length > 500) safePayload = safePayload.Substring(0, 500) + "...";

                    string line = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss} | {1,-9} | {2,-22} | {3,-10} | {4,5} ms | {5} | {6}",
                        ev.Timestamp,
                        ev.Status,
                        ev.TargetName,
                        ev.Method,
                        ev.LatencyMs < 0 ? "-" : ev.LatencyMs.ToString(),
                        ev.Url,
                        ev.Reason
                    );

                    File.AppendAllText(AuditLogPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }

        public static void SaveSettings()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"CurrentMode\": \"" + CurrentMode + "\",");
                sb.AppendLine("  \"MaskApiKeys\": " + MaskApiKeys.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskPasswords\": " + MaskPasswords.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskUserPaths\": " + MaskUserPaths.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskEmails\": " + MaskEmails.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskPhoneNumbers\": " + MaskPhoneNumbers.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskIpAddresses\": " + MaskIpAddresses.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskPostalAddresses\": " + MaskPostalAddresses.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskCreditCards\": " + MaskCreditCards.ToString().ToLower() + ",");
                sb.AppendLine("  \"MaskCustomNames\": " + MaskCustomNames.ToString().ToLower() + ",");
                sb.AppendLine("  \"CustomNames\": \"" + (CustomNames ?? "").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ") + "\",");
                sb.AppendLine("  \"CustomAddresses\": \"" + (CustomAddresses ?? "").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ") + "\"");
                sb.AppendLine("}");
                File.WriteAllText(SettingsPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return;
                string json = File.ReadAllText(SettingsPath, Encoding.UTF8);

                Match mMode = Regex.Match(json, @"""CurrentMode""\s*:\s*""(\w+)""");
                if (mMode.Success)
                {
                    try { CurrentMode = (PolicyMode)Enum.Parse(typeof(PolicyMode), mMode.Groups[1].Value); } catch { }
                }

                MaskApiKeys = GetJsonBool(json, "MaskApiKeys", MaskApiKeys);
                MaskPasswords = GetJsonBool(json, "MaskPasswords", MaskPasswords);
                MaskUserPaths = GetJsonBool(json, "MaskUserPaths", MaskUserPaths);
                MaskEmails = GetJsonBool(json, "MaskEmails", MaskEmails);
                MaskPhoneNumbers = GetJsonBool(json, "MaskPhoneNumbers", MaskPhoneNumbers);
                MaskIpAddresses = GetJsonBool(json, "MaskIpAddresses", MaskIpAddresses);
                MaskPostalAddresses = GetJsonBool(json, "MaskPostalAddresses", MaskPostalAddresses);
                MaskCreditCards = GetJsonBool(json, "MaskCreditCards", MaskCreditCards);
                MaskCustomNames = GetJsonBool(json, "MaskCustomNames", MaskCustomNames);

                Match mNames = Regex.Match(json, @"""CustomNames""\s*:\s*""([^""]*)""");
                if (mNames.Success) CustomNames = mNames.Groups[1].Value;

                Match mAddrs = Regex.Match(json, @"""CustomAddresses""\s*:\s*""([^""]*)""");
                if (mAddrs.Success) CustomAddresses = mAddrs.Groups[1].Value;
            }
            catch { }
        }

        private static bool GetJsonBool(string json, string key, bool fallback)
        {
            Match m = Regex.Match(json, @"""" + key + @"""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
            if (m.Success) return bool.Parse(m.Groups[1].Value);
            return fallback;
        }
    }
}
