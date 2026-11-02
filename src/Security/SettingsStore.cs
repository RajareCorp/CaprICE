using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CaprICE.Hooks;

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
                sb.AppendLine("  \"CustomAddresses\": \"" + Escape(SecurityPolicy.CustomAddresses) + "\",");
                sb.AppendLine("  \"CustomPersonality\": \"" + Escape(SecurityPolicyText(CaprICEHook.CustomPersonality)) + "\",");
                sb.AppendLine("  \"CustomSafety\": \"" + Escape(SecurityPolicyText(CaprICEHook.CustomSafety)) + "\",");
                sb.AppendLine("  \"CustomMachinery\": \"" + Escape(SecurityPolicyText(CaprICEHook.CustomMachinery)) + "\",");
                sb.AppendLine("  \"AppLaunchActions\": \"" + Escape(AppLaunchActions.Serialize()) + "\"");
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

                string value;
                if (TryReadString(json, "CustomNames", out value)) SecurityPolicy.CustomNames = value;
                if (TryReadString(json, "CustomAddresses", out value)) SecurityPolicy.CustomAddresses = value;
                if (TryReadString(json, "CustomPersonality", out value)) CaprICEHook.CustomPersonality = EmptyAsNull(value);
                if (TryReadString(json, "CustomSafety", out value)) CaprICEHook.CustomSafety = EmptyAsNull(value);
                if (TryReadString(json, "CustomMachinery", out value)) CaprICEHook.CustomMachinery = EmptyAsNull(value);
                if (TryReadString(json, "AppLaunchActions", out value)) AppLaunchActions.Load(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to load CaprICE settings: " + ex.Message);
            }
        }

        private static string Bool(bool value)
        {
            return value.ToString().ToLower();
        }

        private static string Escape(string value)
        {
            StringBuilder escaped = new StringBuilder();
            foreach (char character in value ?? "")
            {
                switch (character)
                {
                    case '"': escaped.Append("\\\""); break;
                    case '\\': escaped.Append("\\\\"); break;
                    case '\b': escaped.Append("\\b"); break;
                    case '\f': escaped.Append("\\f"); break;
                    case '\n': escaped.Append("\\n"); break;
                    case '\r': escaped.Append("\\r"); break;
                    case '\t': escaped.Append("\\t"); break;
                    default:
                        if (character < 0x20)
                            escaped.Append("\\u").Append(((int)character).ToString("x4"));
                        else
                            escaped.Append(character);
                        break;
                }
            }
            return escaped.ToString();
        }

        private static bool GetBool(string json, string key, bool fallback)
        {
            Match m = Regex.Match(json, @"""" + key + @"""\s*:\s*(true|false)", RegexOptions.IgnoreCase);
            if (m.Success) return bool.Parse(m.Groups[1].Value);
            return fallback;
        }

        private static bool TryReadString(string json, string key, out string value)
        {
            Match match = Regex.Match(json,
                "\"" + Regex.Escape(key) + "\"\\s*:\\s*(\"(?:\\\\.|[^\"\\\\])*\")",
                RegexOptions.Singleline);
            if (!match.Success)
            {
                value = null;
                return false;
            }

            value = Unescape(match.Groups[1].Value.Substring(1, match.Groups[1].Value.Length - 2));
            return true;
        }

        private static string Unescape(string value)
        {
            StringBuilder result = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (character != '\\' || i + 1 >= value.Length)
                {
                    result.Append(character);
                    continue;
                }

                char escape = value[++i];
                switch (escape)
                {
                    case '"': result.Append('"'); break;
                    case '\\': result.Append('\\'); break;
                    case '/': result.Append('/'); break;
                    case 'b': result.Append('\b'); break;
                    case 'f': result.Append('\f'); break;
                    case 'n': result.Append('\n'); break;
                    case 'r': result.Append('\r'); break;
                    case 't': result.Append('\t'); break;
                    case 'u':
                        if (i + 4 >= value.Length)
                            throw new FormatException("Séquence Unicode JSON incomplète.");
                        result.Append((char)Convert.ToInt32(value.Substring(i + 1, 4), 16));
                        i += 4;
                        break;
                    default:
                        throw new FormatException("Séquence d'échappement JSON invalide.");
                }
            }
            return result.ToString();
        }

        private static string SecurityPolicyText(string value)
        {
            return value ?? string.Empty;
        }

        private static string EmptyAsNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
