using System.Collections.Generic;

namespace CaprICE.Security
{
    /// <summary>
    /// Réglages courants et décisions distinctes pour le contenu (<see cref="EvaluatePayload"/>) et le réseau (<see cref="EvaluateConnection"/>).
    /// La détection est dans <see cref="DlpEngine"/>, la persistance dans <see cref="SettingsStore"/>.
    /// </summary>
    public static class SecurityPolicy
    {
        public static PolicyMode CurrentMode = PolicyMode.MonitorOnly;
        public static bool MaskDlpInPayloads = true;

        // Interrupteurs des règles DLP
        public static bool MaskApiKeys = true;
        public static bool MaskPasswords = true;
        public static bool MaskUserPaths = true;
        public static bool MaskEmails = true;
        public static bool MaskPhoneNumbers = true;
        public static bool MaskIpAddresses = true;
        public static bool MaskPostalAddresses = true;
        public static bool MaskCreditCards = true;
        public static bool MaskCustomNames = true;

        // Données personnelles à toujours masquer
        public static string CustomNames = "";        // ex. "Jean Dupont, Marie Curie"
        public static string CustomAddresses = "";    // ex. "12 rue des Fleurs, 75001 Paris"

        public static string AuditLogPath { get { return AuditLog.Path; } }
        public static string SettingsPath { get { return StoragePaths.Settings; } }

        static SecurityPolicy()
        {
            LoadSettings();
        }

        public static void SaveSettings()
        {
            string error;
            if (!SettingsStore.Save(out error))
                System.Diagnostics.Debug.WriteLine("Unable to save CaprICE settings: " + error);
        }

        public static bool SaveSettings(out string error) { return SettingsStore.Save(out error); }
        public static void LoadSettings() { SettingsStore.Load(); }

        public static InterceptStatus EvaluatePayload(ref string payload, out List<string> violations, out string reason)
        {
            violations = new List<string>();
            reason = string.Empty;
            violations = DlpEngine.Scan(payload);

            if (violations.Count > 0 && CurrentMode == PolicyMode.EnforceDlp)
            {
                if (MaskDlpInPayloads)
                {
                    payload = DlpEngine.Redact(payload);
                    reason = "Sensitive data masked before transmission (" + string.Join(", ", violations.ToArray()) + ")";
                    return InterceptStatus.DlpMasked;
                }

                reason = "Blocked by DLP policy (" + string.Join(", ", violations.ToArray()) + ")";
                return InterceptStatus.Blocked;
            }

            reason = violations.Count > 0 ? "Sensitive data detected (monitor mode)" : "No sensitive data detected";
            return InterceptStatus.Allowed;
        }

        public static InterceptStatus EvaluateConnection(string url, out string reason)
        {
            bool isExternal = TargetDetector.IsUrlExternal(url);
            if (isExternal && CurrentMode == PolicyMode.BlockExternal)
            {
                reason = "Blocked by Strict Local policy (external connection denied)";
                return InterceptStatus.Blocked;
            }

            if (isExternal && CurrentMode == PolicyMode.AskConfirmation)
            {
                reason = "Approval required before external connection";
                return InterceptStatus.PendingApproval;
            }

            reason = "Allowed";
            return InterceptStatus.Allowed;
        }
    }
}
