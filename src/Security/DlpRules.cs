using System.Text.RegularExpressions;

namespace CaprICE.Security
{
    /// <summary>Expressions régulières intégrées de détection des données sensibles.</summary>
    internal static class DlpRules
    {
        // Built-in Regex Patterns
        public static readonly Regex ApiKeys = new Regex(@"\b(sk-[a-zA-Z0-9_\-]{20,}|anthropic-[a-zA-Z0-9_\-]{20,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex Secrets = new Regex(@"(api[_-]?key|access[_-]?token|bearer|secret)\s*[:=]\s*[""']?([a-zA-Z0-9_\-]{16,})[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex Passwords = new Regex(@"(password|passwd|pwd|motdepasse)\s*[:=]\s*[""']?([^""'\s]{4,})[""']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex UserPaths = new Regex(@"[A-Za-z]:\\Users\\[a-zA-Z0-9_.\-]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex Emails = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
        public static readonly Regex CreditCards = new Regex(@"\b(?:\d{4}[- ]?){3}\d{4}\b", RegexOptions.Compiled);

        // Phone Numbers: French (+33 / 06 / 07 / 01...) and International formats
        public static readonly Regex Phones = new Regex(@"(?:(?:\+|00)33|0)\s*[1-9](?:[\s.-]*\d{2}){4}|\b(?:\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled);

        // IP Addresses (IPv4 and standard IPv6)
        public static readonly Regex Ipv4 = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b", RegexOptions.Compiled);
        public static readonly Regex Ipv6 = new Regex(@"\b(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\b", RegexOptions.Compiled);

        // Postal Addresses: "12 rue de la Paix", "45 avenue Foch", "10 boulevard Saint-Michel", etc.
        public static readonly Regex PostalAddresses = new Regex(@"\b\d{1,5}(?:\s*(?:bis|ter))?\s+(?:rue|avenue|av\.|boulevard|bd|chemin|allée|impasse|route|place|quai|square|voie|st\.|street|road|rd\.|avenue|blvd)\s+[A-Za-zÀ-ÿ0-9'\-\s]{3,35}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex PostalCodes = new Regex(@"\b(?:code\s*postal\s*[:=]?\s*|[A-Z]{2}-)?\d{5}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}
