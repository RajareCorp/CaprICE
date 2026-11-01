using System;
using System.Linq;
using System.Reflection;
using CaprICE.Hooks;

namespace CaprICE.Services
{
    public static class PersonaReader
    {
        private static Type GetBrainType()
        {
            if (CaprICEHook.CapriAssembly != null)
                return CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");

            return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes())
                        .FirstOrDefault(t => t.FullName == "CapriDesktop.Brain");
        }

        private static string GetStaticStringFieldOrProp(string name)
        {
            try
            {
                Type brainType = GetBrainType();
                if (brainType == null) return string.Empty;

                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

                FieldInfo field = brainType.GetField(name, flags);
                if (field != null) return field.GetValue(null) as string;

                PropertyInfo prop = brainType.GetProperty(name, flags);
                if (prop != null) return prop.GetValue(null) as string;
            }
            catch { }

            return string.Empty;
        }

        public static string GetDefaultPersonality() => FormatWinForms(GetStaticStringFieldOrProp("DefaultPersonality"));
        public static string GetSafety() => FormatWinForms(GetStaticStringFieldOrProp("Safety"));
        public static string GetMachinery() => FormatWinForms(GetStaticStringFieldOrProp("Machinery"));

        public static string GetOriginalCapriPrompt()
        {
            string persona = GetStaticStringFieldOrProp("Persona");
            return string.IsNullOrEmpty(persona) ? "(Impossible de lire Brain.Persona)" : FormatWinForms(persona);
        }

        private static string FormatWinForms(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}