namespace CaprICE.Security
{
    /// <summary>Classe une URL : locale ou externe, et fournisseur IA reconnu.</summary>
    public static class TargetDetector
    {
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
    }
}
