using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CaprICE.Security
{
    /// <summary>Détection (Scan) et masquage (Redact) des données sensibles selon les réglages actifs.</summary>
    internal static class DlpEngine
    {
        private static readonly char[] ListSeparators = { ',', ';', '\r', '\n' };

        /// <summary>Retourne la liste des types de données sensibles trouvées dans le texte.</summary>
        public static List<string> Scan(string payload)
        {
            List<string> violations = new List<string>();
            if (string.IsNullOrEmpty(payload)) return violations;

            CheckRule(DlpRules.ApiKeys, "API key", payload, violations, SecurityPolicy.MaskApiKeys);
            CheckRule(DlpRules.Secrets, "Token / Secret", payload, violations, SecurityPolicy.MaskApiKeys);
            CheckRule(DlpRules.Passwords, "Password", payload, violations, SecurityPolicy.MaskPasswords);
            CheckRule(DlpRules.UserPaths, "Windows user path", payload, violations, SecurityPolicy.MaskUserPaths);
            CheckRule(DlpRules.Emails, "Email address", payload, violations, SecurityPolicy.MaskEmails);
            CheckRule(DlpRules.CreditCards, "Credit card number", payload, violations, SecurityPolicy.MaskCreditCards);
            CheckRule(DlpRules.Phones, "Phone number", payload, violations, SecurityPolicy.MaskPhoneNumbers);
            CheckRule(DlpRules.Ipv4, "IPv4 address", payload, violations, SecurityPolicy.MaskIpAddresses);
            CheckRule(DlpRules.Ipv6, "IPv6 address", payload, violations, SecurityPolicy.MaskIpAddresses);
            CheckRule(DlpRules.PostalAddresses, "Street address", payload, violations, SecurityPolicy.MaskPostalAddresses);
            CheckRule(DlpRules.PostalCodes, "Postal code", payload, violations, SecurityPolicy.MaskPostalAddresses);

            // Noms & prénoms personnalisés
            if (SecurityPolicy.MaskCustomNames)
            {
                foreach (string name in SplitList(SecurityPolicy.CustomNames, 2))
                {
                    if (NameRegex(name).IsMatch(payload))
                        violations.Add("Custom name");
                }
            }

            // Adresses personnelles personnalisées
            if (SecurityPolicy.MaskPostalAddresses)
            {
                foreach (string addr in SplitList(SecurityPolicy.CustomAddresses, 3))
                {
                    if (AddressRegex(addr).IsMatch(payload))
                        violations.Add("Custom address");
                }
            }

            return violations;
        }

        /// <summary>Remplace les données sensibles par des marqueurs [REDACTED:...].</summary>
        public static string Redact(string payload)
        {
            if (SecurityPolicy.MaskApiKeys)
            {
                payload = DlpRules.ApiKeys.Replace(payload, "[REDACTED:API_KEY]");
                payload = DlpRules.Secrets.Replace(payload, "$1: \"[REDACTED:TOKEN]\"");
            }
            if (SecurityPolicy.MaskPasswords) payload = DlpRules.Passwords.Replace(payload, "$1: \"[REDACTED:PASSWORD]\"");
            if (SecurityPolicy.MaskUserPaths) payload = DlpRules.UserPaths.Replace(payload, "C:\\Users\\[REDACTED_USER]");
            if (SecurityPolicy.MaskEmails) payload = DlpRules.Emails.Replace(payload, "[REDACTED:EMAIL]");
            if (SecurityPolicy.MaskCreditCards) payload = DlpRules.CreditCards.Replace(payload, "[REDACTED:CARD]");
            if (SecurityPolicy.MaskPhoneNumbers) payload = DlpRules.Phones.Replace(payload, "[REDACTED:PHONE]");
            if (SecurityPolicy.MaskIpAddresses)
            {
                payload = DlpRules.Ipv4.Replace(payload, "[REDACTED:IP]");
                payload = DlpRules.Ipv6.Replace(payload, "[REDACTED:IPV6]");
            }
            if (SecurityPolicy.MaskPostalAddresses)
            {
                payload = DlpRules.PostalAddresses.Replace(payload, "[REDACTED:ADDRESS]");
                payload = DlpRules.PostalCodes.Replace(payload, "[REDACTED:ZIP]");
            }

            if (SecurityPolicy.MaskCustomNames)
            {
                foreach (string name in SplitList(SecurityPolicy.CustomNames, 2))
                    payload = NameRegex(name).Replace(payload, "[REDACTED:NAME]");
            }

            if (SecurityPolicy.MaskPostalAddresses)
            {
                foreach (string addr in SplitList(SecurityPolicy.CustomAddresses, 3))
                    payload = AddressRegex(addr).Replace(payload, "[REDACTED:ADDRESS]");
            }

            return payload;
        }

        private static void CheckRule(Regex rx, string name, string payload, List<string> violations, bool enabled)
        {
            if (!enabled) return;
            MatchCollection matches = rx.Matches(payload);
            if (matches.Count > 0)
            {
                violations.Add(name + " (" + matches.Count + " match" + (matches.Count > 1 ? "es" : "") + ")");
            }
        }

        /// <summary>Découpe une liste saisie par l'utilisateur et ignore les entrées trop courtes.</summary>
        private static IEnumerable<string> SplitList(string raw, int minLength)
        {
            if (string.IsNullOrWhiteSpace(raw)) yield break;
            foreach (string item in raw.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                string value = item.Trim();
                if (value.Length >= minLength) yield return value;
            }
        }

        private static Regex NameRegex(string name)
        {
            return new Regex(@"\b" + Regex.Escape(name) + @"\b", RegexOptions.IgnoreCase);
        }

        private static Regex AddressRegex(string address)
        {
            return new Regex(Regex.Escape(address), RegexOptions.IgnoreCase);
        }
    }
}
