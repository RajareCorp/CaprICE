namespace CaprICE.Hooks
{
    /// <summary>Hook de Brain.Persona : remplace le prompt système de Capri.</summary>
    public static class PersonaHooks
    {
        public static void OnGetPersonaPostfix(ref string __result)
        {
            if (!string.IsNullOrEmpty(CaprICEHook.CustomSystemPrompt))
            {
                // Remplacer complètement le prompt initial de Capri par le prompt personnalisé
                __result = CaprICEHook.CustomSystemPrompt;
            }
        }
    }
}
