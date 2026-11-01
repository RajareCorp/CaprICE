using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CaprICE.Services;

namespace CaprICE.Hooks
{
    /// <summary>Accès par réflexion à l'état interne de CapriDesktop.Brain (historique, mémoire).</summary>
    internal static class BrainReflection
    {
        private const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>Vide l'historique de conversation d'une instance de Brain.</summary>
        public static bool ClearBrainHistory(object brainInstance = null)
        {
            try
            {
                IList history = GetHistoryField(brainInstance);
                if (history != null)
                {
                    history.Clear();
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Remplace le contenu du message "system" de l'historique actif par le prompt personnalisé.
        /// </summary>
        public static void ApplyCustomSystemPrompt(object brainInstance, string customPrompt)
        {
            if (string.IsNullOrEmpty(customPrompt) || brainInstance == null) return;

            try
            {
                IList history = GetHistoryField(brainInstance);
                if (history == null) return;

                foreach (object msg in history)
                {
                    if (msg == null) continue;

                    PropertyInfo roleProp = msg.GetType().GetProperty("Role");
                    PropertyInfo contentProp = msg.GetType().GetProperty("Content");

                    string role = roleProp != null ? roleProp.GetValue(msg, null) as string : null;
                    if (role == "system" && contentProp != null)
                    {
                        contentProp.SetValue(msg, customPrompt, null);
                    }
                }
                // NOTE : si l'historique ne contient pas encore de message "system", rien n'est inséré ;
                // le prompt personnalisé est alors appliqué via le hook Brain.Persona.
            }
            catch { }
        }

        /// <summary>Assemble la mémoire persistante et l'historique envoyés au modèle (pour audit).</summary>
        public static string ExtractContextAndMemory(object brainInstance, out int turnCount)
        {
            turnCount = 0;
            if (brainInstance == null) return null;

            StringBuilder sb = new StringBuilder();
            try
            {
                Type brainType = brainInstance.GetType();

                // 1. Mémoire long terme (memories.md / me.md)
                FieldInfo memField = brainType.GetField("MemoryPath", BindingFlags.NonPublic | BindingFlags.Static);
                string memPath = memField != null ? memField.GetValue(null) as string : null;
                if (string.IsNullOrEmpty(memPath) || !File.Exists(memPath))
                {
                    memPath = MemoryManager.GetMemoryFilePath();
                }

                if (!string.IsNullOrEmpty(memPath) && File.Exists(memPath))
                {
                    sb.AppendLine("=== MÉMOIRE PERSISTANTE CAPRI (" + Path.GetFileName(memPath) + ") ===");
                    sb.AppendLine(File.ReadAllText(memPath));
                    sb.AppendLine();
                }

                // 2. Tours de conversation
                PropertyInfo histProp = brainType.GetProperty("History", NonPublicInstance);
                if (histProp != null)
                {
                    IEnumerable histList = histProp.GetValue(brainInstance, null) as IEnumerable;
                    if (histList != null)
                    {
                        List<string> turns = new List<string>();
                        foreach (object item in histList)
                        {
                            if (item == null) continue;

                            Type msgType = item.GetType();
                            PropertyInfo roleProp = msgType.GetProperty("Role");
                            PropertyInfo contentProp = msgType.GetProperty("Content");
                            string role = roleProp != null ? roleProp.GetValue(item, null) as string : "msg";
                            string content = contentProp != null ? contentProp.GetValue(item, null) as string : "";
                            turns.Add("[" + (role ?? "").ToUpperInvariant() + "] " + content);
                        }

                        turnCount = turns.Count;
                        if (turns.Count > 0)
                        {
                            sb.AppendLine("=== HISTORIQUE DE CONVERSATION ENVOYÉ (" + turns.Count + " tours) ===");
                            foreach (string t in turns) sb.AppendLine(t);
                        }
                    }
                }
            }
            catch { }

            return sb.Length > 0 ? sb.ToString() : "(Mémoire et historique vides pour l'instant)";
        }

        private static IList GetHistoryField(object brainInstance)
        {
            if (brainInstance == null) return null;
            FieldInfo field = brainInstance.GetType().GetField("history", NonPublicInstance);
            return field != null ? field.GetValue(brainInstance) as IList : null;
        }
    }
}
