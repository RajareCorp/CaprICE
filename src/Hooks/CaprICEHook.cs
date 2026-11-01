using System;
using System.Reflection;
using System.Windows.Forms;
using HarmonyLib;
using CaprICE.Services;

namespace CaprICE.Hooks
{
    /// <summary>
    /// Point d'entrée de l'injection : état partagé (proxy, prompts modulaire/système personnalisé)
    /// et installation des patchs Harmony sur l'assembly Capri.
    /// Les callbacks eux-mêmes sont dans BrainHooks, VoiceHooks et PersonaHooks.
    /// </summary>
    public static class CaprICEHook
    {
        public static Assembly CapriAssembly = null;
        public static string CustomProxyUrl = null;

        // Variables pour surcharger chaque section du prompt individuellement
        public static string CustomPersonality = null;
        public static string CustomSafety = null;
        public static string CustomMachinery = null;

        /// <summary>
        /// Reconstitue le System Prompt global dynamiquement ou renvoie null si aucune surcharge n'est active.
        /// </summary>
        public static string CustomSystemPrompt
        {
            get
            {
                if (CustomPersonality == null && CustomSafety == null && CustomMachinery == null)
                    return null;

                string p = CustomPersonality ?? PersonaReader.GetDefaultPersonality();
                string s = CustomSafety ?? PersonaReader.GetSafety();
                string m = CustomMachinery ?? PersonaReader.GetMachinery();

                return $"{p}\n{s}\n{m}";
            }
            set
            {
                // Rétrocompatibilité : réinitialise ou assigne le bloc complet à Personality
                CustomPersonality = value;
                CustomSafety = null;
                CustomMachinery = null;
            }
        }

        public static bool IsInitialized { get; private set; }

        private const BindingFlags AnyStatic = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        public static void Initialize(Assembly capriAssembly)
        {
            if (IsInitialized) return;

            CapriAssembly = capriAssembly;

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

        public static bool ClearBrainHistory(object brainInstance = null)
        {
            try
            {
                Type brainType = CapriAssembly != null ? CapriAssembly.GetType("CapriDesktop.Brain") : null;
                if (brainType == null) return false;

                if (brainInstance == null)
                {
                    FieldInfo keeperField = brainType.GetField("Keeper", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (keeperField != null) brainInstance = keeperField.GetValue(null);
                }

                return BrainReflection.ClearBrainHistory(brainInstance);
            }
            catch { }
            return false;
        }

        public static string GetActiveMemoryPath()
        {
            return BrainReflection.GetActiveMemoryPath();
        }

        private static void PatchBrain(Harmony harmony, Type brainType)
        {
            if (brainType == null) return;


            // Brain.Greeting(int hour) -> Modification du message d'accueil initial
            MethodInfo greeting = brainType.GetMethod("Greeting", AnyStatic);
            if (greeting != null)
            {
                harmony.Patch(greeting,
                    postfix: Hook(typeof(BrainHooks), "OnBrainGreetingPostfix"));
            }

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

            MethodInfo think = brainType.GetMethod("Think", BindingFlags.NonPublic | BindingFlags.Instance);
            if (think != null)
            {
                harmony.Patch(think,
                    prefix: Hook(typeof(BrainHooks), "OnBrainThinkPrefix"),
                    finalizer: Hook(typeof(BrainHooks), "OnBrainThinkFinalizer"));
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
                harmony.Patch(open,
                    prefix: Hook(typeof(VoiceHooks), "OnVoiceOpenPrefix"),
                    postfix: Hook(typeof(VoiceHooks), "OnVoiceOpenPostfix"));
            }
        }

        private static HarmonyMethod Hook(Type owner, string methodName)
        {
            return new HarmonyMethod(owner.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static));
        }
    }
}