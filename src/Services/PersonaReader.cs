using System;
using System.Linq;
using System.Reflection;
using CaprICE.Hooks;

namespace CaprICE.Services
{
    /// <summary>Lit le prompt système d'origine de Capri (Brain.Persona) sans l'override CaprICE.</summary>
    public static class PersonaReader
    {
        public static string GetOriginalCapriPrompt()
        {
            try
            {
                Type brainType = null;

                if (CaprICEHook.CapriAssembly != null)
                {
                    brainType = CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");
                }
                else
                {
                    brainType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.FullName == "CapriDesktop.Brain");
                }

                if (brainType != null)
                {
                    PropertyInfo personaProp = brainType.GetProperty("Persona", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    if (personaProp != null)
                    {
                        // Désactive temporairement l'override pour lire la vraie valeur d'origine
                        string currentCustom = CaprICEHook.CustomSystemPrompt;
                        CaprICEHook.CustomSystemPrompt = null;

                        string originalPersona = personaProp.GetValue(null) as string;

                        // Restaure la valeur personnalisée
                        CaprICEHook.CustomSystemPrompt = currentCustom;

                        if (!string.IsNullOrEmpty(originalPersona))
                        {
                            // WinForms : conversion des sauts de ligne Linux (\n) en Windows (\r\n)
                            return originalPersona.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                        }
                    }
                }
            }
            catch { }

            return "(Impossible de lire Brain.Persona depuis Capri)";
        }
    }
}
