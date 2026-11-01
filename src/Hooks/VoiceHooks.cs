using System;
using System.Net;
using CaprICE.Security;
using CaprICE.UI;

namespace CaprICE.Hooks
{
    /// <summary>Hook de Voice.Open : proxy amont et notification des sessions ElevenLabs.</summary>
    public static class VoiceHooks
    {
        public static void OnVoiceOpenPostfix(ref HttpWebRequest __result, string path, string key, string method)
        {
            if (__result != null && !string.IsNullOrEmpty(CaprICEHook.CustomProxyUrl))
            {
                try { __result.Proxy = new WebProxy(CaprICEHook.CustomProxyUrl); } catch { }
            }

            InterceptEvent ev = new InterceptEvent
            {
                Url = "https://api.elevenlabs.io" + path,
                TargetName = "ElevenLabs Voice",
                IsExternal = true,
                Method = method,
                Status = InterceptStatus.Allowed,
                Reason = "Session audio / TTS ouverte"
            };
            CaprICEForm.NotifyEvent(ev);
        }
    }
}
