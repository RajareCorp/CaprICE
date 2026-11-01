using System;

namespace CaprICE.Security
{
    /// <summary>Classe une URL à partir de son URI et de son hôte réel.</summary>
    public static class TargetDetector
    {
        public static bool IsUrlExternal(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return true;

            string host = uri.Host.TrimEnd('.').ToLowerInvariant();
            return !uri.IsLoopback
                && host != "localhost"
                && !host.EndsWith(".localhost", StringComparison.Ordinal);
        }

        public static string DetectTarget(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return "Cible inconnue";

            string host = uri.Host.TrimEnd('.').ToLowerInvariant();
            if (!IsUrlExternal(url))
            {
                if (uri.Port == 11434) return "Ollama (Local)";
                if (uri.Port == 1234) return "LM Studio (Local)";
                if (uri.Port == 1337) return "Jan (Local)";
                if (uri.Port == 5001) return "KoboldCpp (Local)";
                if (uri.Port == 8080) return "llama.cpp (Local)";
                return "Serveur local" + (uri.IsDefaultPort ? "" : " :" + uri.Port);
            }

            if (IsHostOrSubdomain(host, "api.openai.com")) return "OpenAI (Cloud)";
            if (IsHostOrSubdomain(host, "api.anthropic.com")) return "Anthropic Claude (Cloud)";
            if (IsHostOrSubdomain(host, "api.deepseek.com")) return "DeepSeek (Cloud)";
            if (IsHostOrSubdomain(host, "generativelanguage.googleapis.com")) return "Google Gemini (Cloud)";
            if (IsHostOrSubdomain(host, "integrate.api.nvidia.com")) return "NVIDIA NIM (Cloud)";
            if (IsHostOrSubdomain(host, "openrouter.ai")) return "OpenRouter (Cloud)";
            if (IsHostOrSubdomain(host, "elevenlabs.io")) return "ElevenLabs Voice (Cloud)";
            if (IsHostOrSubdomain(host, "github.com")) return "GitHub";
            return "API externe (" + host + ")";
        }

        public static string SanitizeUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return "URI invalide";

            UriBuilder safe = new UriBuilder(uri)
            {
                UserName = string.Empty,
                Password = string.Empty,
                Query = string.Empty,
                Fragment = string.Empty
            };
            return safe.Uri.AbsoluteUri.TrimEnd('/');
        }

        private static bool IsHostOrSubdomain(string host, string domain)
        {
            return host == domain || host.EndsWith("." + domain, StringComparison.Ordinal);
        }
    }
}
