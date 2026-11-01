using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CaprICE.Security
{
    /// <summary>Lecture / écriture de caprice_settings.json (format JSON minimal, sans dépendance).</summary>
    internal static class SettingsStore
    {
        public static bool Save(out string error)
        {
            error = null;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("{");
                sb.AppendLine("  \"CurrentMode\": \"" + SecurityPolicy.CurrentMode + "\",");
                sb.AppendLine("  \"MaskApiKeys\": " + Bool(SecurityPolicy.MaskApiKeys) + ",");
                sb.AppendLine("  \"MaskPasswords\": " + Bool(SecurityPolicy.MaskPasswords) + ",");
                sb.AppendLine("  \"MaskUserPaths\": " + Bool(SecurityPolicy.MaskUserPaths) + ",");
                sb.AppendLine("  \"MaskEmails\": " + Bool(SecurityPolicy.MaskEmails) + ",");
                sb.AppendLine("  \"MaskPhoneNumbers\": " + Bool(SecurityPolicy.MaskPhoneNumbers) + ",");
                sb.AppendLine("  \"MaskIpAddresses\": " + Bool(SecurityPolicy.MaskIpAddresses) + ",");
                sb.AppendLine("  \"MaskPostalAddresses\": " + Bool(SecurityPolicy.MaskPostalAddresses) + ",");
                sb.AppendLine("  \"MaskCreditCards\": " + Bool(SecurityPolicy.MaskCreditCards) + ",");
                sb.AppendLine("  \"MaskCustomNames\": " + Bool(SecurityPolicy.MaskCustomNames) + ",");
                sb.AppendLine("  \"CustomNames\": \"" + Escape(SecurityPolicy.CustomNames) + "\",");
                sb.AppendLine("  \"CustomAddresses\": \"" + Escape(SecurityPolicy.CustomAddresses) + "\"");
                sb.AppendLine("}");
                File.WriteAllText(StoragePaths.Settings, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(StoragePaths.Settings)) return;
                string json = File.ReadAllText(StoragePaths.Settings, Encoding.UTF8);

                Match mMode = Regex.Match(json, @"""CurrentMode""\s*:\s*""(\w+)""");
                if (mMode.Success)
                {
                    try { SecurityPolicy.CurrentMode = (PolicyMode)Enum.Parse(typeof(PolicyMode), mMode.Groups[1].Value); } catch { }
                }

                SecurityPolicy.MaskApiKeys = GetBool(json, "MaskApiKeys", SecurityPolicy.MaskApiKeys);
                SecurityPolicy.MaskPasswords = GetBool(json, "MaskPasswords", SecurityPolicy.MaskPasswords);
                SecurityPolicy.MaskUserPaths = GetBool(json, "MaskUserPaths", SecurityPolicy.MaskUserPaths);
                SecurityPolicy.MaskEmails = GetBool(json, "MaskEmails", SecurityPolicy.MaskEmails);
                SecurityPolicy.MaskPhoneNumbers = GetBool(json, "MaskPhoneNumbers", SecurityPolicy.MaskPhoneNumbers);
                SecurityPolicy.MaskIpAddresses = GetBool(json, "MaskIpAddresses", SecurityPolicy.MaskIpAddresses);
                SecurityPolicy.MaskPostalAddresses = GetBool(json, "MaskPostalAddresses", SecurityPolicy.MaskPostalAddresses);
                SecurityPolicy.MaskCreditCards = GetBool(json, "MaskCreditCards", SecurityPolicy.MaskCreditCards);
                SecurityPolicy.MaskCustomNames = GetBool(json, "MaskCustomNames", SecurityPolicy.MaskCustomNames);

                Match mNames = Regex.Match(json, @"""CustomNames""\s*:\s*""([^""]*)""");
                if (mNames.Success) SecurityPolicy.CustomNames = mNames.Groups[1].Value;

                Match mAddrs = Regex.Match(json, @"""CustomAddresses""\s*:\s*""([^""]*)""");
                if (mAddrs.Success) SecurityPolicy.CustomAddresses = mAddrs.Groups[1].Value;
            }
            catch { }
        }

        private static string Bool(bool value)
        {
            return value.ToString().ToLower();
        }

        private static string Escape(string value)
        {
            return (value ?? "").Replace("\"", "\\\"").Replace("\r", " ").Replace("\n", " ");
        }

        private static bool GetBool(string json, string key, bool fallback)
        {
            Match m = Regex.Match(json, @"""" + key + @"""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
            if (m.Success) return bool.Parse(m.Groups[1].Value);
            return fallback;
        }
    }
}
