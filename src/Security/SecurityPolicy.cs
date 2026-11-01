using System.Collections.Generic;

namespace CaprICE.Security
{
    /// <summary>
    /// Réglages courants (mode, règles DLP actives, données perso) et décision <see cref="Evaluate"/>.
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

        public static void SaveSettings() { SettingsStore.Save(); }
        public static void LoadSettings() { SettingsStore.Load(); }

        public static InterceptStatus Evaluate(string url, ref string payload, out List<string> violations, out string reason)
        {
            violations = new List<string>();
            reason = string.Empty;
            bool isExternal = TargetDetector.IsUrlExternal(url);

            // 1. Mode Strict Local
            if (CurrentMode == PolicyMode.BlockExternal && isExternal)
            {
                reason = "Bloqué par Politique Strict Local (IA Cloud externe interdite)";
                return InterceptStatus.Blocked;
            }

            // 2. Analyse DLP
            violations = DlpEngine.Scan(payload);

            // 3. Application de la règle DLP
            if (violations.Count > 0 && CurrentMode == PolicyMode.EnforceDlp)
            {
                if (MaskDlpInPayloads)
                {
                    payload = DlpEngine.Redact(payload);
                    reason = "Données sensibles masquées avant transmission (" + string.Join(", ", violations.ToArray()) + ")";
                    return InterceptStatus.DlpMasked;
                }

                reason = "Bloqué par Politique DLP (" + string.Join(", ", violations.ToArray()) + ")";
                return InterceptStatus.Blocked;
            }

            // 4. Confirmation manuelle
            if (CurrentMode == PolicyMode.AskConfirmation && isExternal)
            {
                reason = "En attente d'approbation manuelle";
                return InterceptStatus.PendingApproval;
            }

            reason = "Autorisé";
            return InterceptStatus.Allowed;
        }
    }
}
