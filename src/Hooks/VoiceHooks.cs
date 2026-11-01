using System;
using System.Net;
using System.Reflection;
using CaprICE.Security;

namespace CaprICE.Hooks
{
    /// <summary>Applique les politiques réseau Capri aux connexions vocales.</summary>
    public static class VoiceHooks
    {
        public static bool OnVoiceOpenPrefix(string path, string key, string method)
        {
            string url = GetRequestUrl(path);
            if (!BrainHooks.AuthorizeConnection(url, "VOICE " + (method ?? "OPEN")))
                throw new WebException("[CaprICE Security Guard] Voice connection blocked.");
            return true;
        }

        public static void OnVoiceOpenPostfix(ref HttpWebRequest __result, string path, string key, string method)
        {
            if (__result != null && !string.IsNullOrEmpty(CaprICEHook.CustomProxyUrl))
            {
                try { __result.Proxy = new WebProxy(CaprICEHook.CustomProxyUrl); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Unable to apply CaprICE voice proxy: " + ex.Message); }
            }
        }

        private static string GetRequestUrl(string path)
        {
            Type voiceType = CaprICEHook.CapriAssembly == null ? null
                : CaprICEHook.CapriAssembly.GetType("CapriDesktop.Voice");
            FieldInfo hostField = voiceType == null ? null
                : voiceType.GetField("Host", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            string host = hostField == null ? null : hostField.GetValue(null) as string;
            if (string.IsNullOrWhiteSpace(host)) host = "https://api.elevenlabs.io";

            Uri requestUri;
            string actualUrl = host + (path ?? string.Empty);
            return Uri.TryCreate(actualUrl, UriKind.Absolute, out requestUri) ? requestUri.AbsoluteUri : host;
        }
    }
}
