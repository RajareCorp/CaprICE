using System;
using System.Reflection;
using System.Windows.Forms;
using HarmonyLib;

namespace CaprICE.Hooks
{
    /// <summary>
    /// Point d'entrée de l'injection : état partagé (proxy, prompt système personnalisé)
    /// et installation des patchs Harmony sur l'assembly Capri.
    /// Les callbacks eux-mêmes sont dans BrainHooks, VoiceHooks et PersonaHooks.
    /// </summary>
    public static class CaprICEHook
    {
        public static Assembly CapriAssembly = null;
        public static string CustomProxyUrl = null;
        public static string CustomSystemPrompt = null;
        public static bool IsInitialized { get; private set; }

        private const BindingFlags AnyStatic = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        public static void Initialize(Assembly capriAssembly)
        {
            if (IsInitialized) return;

            try
            {
                Harmony harmony = new Harmony("com.caprice.security.interceptor");

                PatchBrain(harmony, capriAssembly.GetType("CapriDesktop.Brain"));
                PatchVoice(harmony, capriAssembly.GetType("CapriDesktop.Voice"));

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("CaprICE Hook Warning: " + ex.Message, "CaprICE Init",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void PatchBrain(Harmony harmony, Type brainType)
        {
            if (brainType == null) return;

            // Brain.Persona (getter) -> remplacement du prompt système
            PropertyInfo personaProp = brainType.GetProperty("Persona", AnyStatic);
            if (personaProp != null && personaProp.GetMethod != null)
            {
                harmony.Patch(personaProp.GetMethod,
                    postfix: Hook(typeof(PersonaHooks), "OnGetPersonaPostfix"));
            }

            // Brain.Create(string url, int timeout) -> blocage / proxy
            MethodInfo create = brainType.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Static);
            if (create != null)
            {
                harmony.Patch(create,
                    prefix: Hook(typeof(BrainHooks), "OnBrainCreatePrefix"),
                    postfix: Hook(typeof(BrainHooks), "OnBrainCreatePostfix"));
            }

            // Brain.Ask(string text, bool followUps, out string followLine) -> DLP, contexte, confirmation
            MethodInfo ask = brainType.GetMethod("Ask", BindingFlags.NonPublic | BindingFlags.Instance, null,
                new Type[] { typeof(string), typeof(bool), typeof(string).MakeByRefType() }, null);
            if (ask != null)
            {
                harmony.Patch(ask,
                    prefix: Hook(typeof(BrainHooks), "OnBrainAskPrefix"),
                    postfix: Hook(typeof(BrainHooks), "OnBrainAskPostfix"));
            }

            // Brain.Log(string url, long took, string what) -> journalisation des endpoints
            MethodInfo log = brainType.GetMethod("Log", AnyStatic);
            if (log != null)
            {
                harmony.Patch(log, prefix: Hook(typeof(BrainHooks), "OnBrainLogPrefix"));
            }
        }

        private static void PatchVoice(Harmony harmony, Type voiceType)
        {
            if (voiceType == null) return;

            MethodInfo open = voiceType.GetMethod("Open", AnyStatic);
            if (open != null)
            {
                harmony.Patch(open, postfix: Hook(typeof(VoiceHooks), "OnVoiceOpenPostfix"));
            }
        }

        private static HarmonyMethod Hook(Type owner, string methodName)
        {
            return new HarmonyMethod(owner.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static));
        }
    }
}
