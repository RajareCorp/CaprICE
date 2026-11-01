using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using CaprICE.Services;
using CaprICE.Security;
using System.Runtime.CompilerServices;

namespace CaprICE.Hooks
{
    /// <summary>Accès par réflexion à l'état interne de CapriDesktop.Brain (historique, mémoire).</summary>
    internal static class BrainReflection
    {
        private const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>Vide l'historique de conversation d'une instance de Brain.</summary>
        public static bool ClearBrainHistory(object brainInstance = null)
        {
            Type brainType = brainInstance != null ? brainInstance.GetType()
                : CaprICEHook.CapriAssembly == null ? null : CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");
            if (brainType == null) return false;

            IList history = GetHistoryField(brainInstance);
            bool cleared = false;
            if (history != null)
            {
                object gate = GetInstanceField(brainType, brainInstance, "gate");
                if (gate == null) history.Clear();
                else lock (gate) history.Clear();
                cleared = true;
            }

            cleared |= ClearStaticCollection(brainType, "sightings", "seeing");
            cleared |= ClearStaticCollection(brainType, "dug", null);

            MethodInfo unsee = brainType.GetMethod("Unsee", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (unsee != null)
            {
                unsee.Invoke(null, null);
                cleared = true;
            }
            return cleared;
        }

        /// <summary>
        /// Remplace le contenu du message "system" de l'historique actif par le prompt personnalisé.
        /// </summary>
        public static void ApplyCustomSystemPrompt(object brainInstance, string customPrompt)
        {
            if (string.IsNullOrEmpty(customPrompt) || brainInstance == null) return;

            IList history = GetHistoryField(brainInstance);
            if (history == null) return;

            foreach (object msg in history)
            {
                if (msg == null) continue;
                string role = ReadStringMember(msg, "Role");
                if (role == "system") WriteStringMember(msg, "Content", customPrompt);
            }
            // Si aucun message system n'existe encore, Brain.Persona applique le prompt au prochain tour.
        }

        public static string GetActiveMemoryPath()
        {
            Type brainType = CaprICEHook.CapriAssembly == null ? null
                : CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");
            return brainType == null ? null : GetStaticField(brainType, "MemoryPath") as string;
        }

        public static string GetTargetUrl(object brainInstance)
        {
            if (brainInstance == null) return null;
            Type type = brainInstance.GetType();
            Type brainType = CaprICEHook.CapriAssembly == null ? type
                : CaprICEHook.CapriAssembly.GetType("CapriDesktop.Brain");
            object remote = GetInstanceField(type, brainInstance, "Remote");
            if (remote != null)
            {
                string kind = ReadStringMember(remote, "Kind");
                if (string.Equals(kind, "local", StringComparison.OrdinalIgnoreCase))
                    return ReadStringMember(remote, "Host");

                switch ((kind ?? "").ToLowerInvariant())
                {
                    case "openai": return AppendPath(GetBrainStatic(brainType, "OpenAi"), "/v1/chat/completions");
                    case "anthropic": return AppendPath(GetBrainStatic(brainType, "Anthropic"), "/v1/messages");
                    case "deepseek": return AppendPath(GetBrainStatic(brainType, "DeepSeek"), "/chat/completions");
                    case "google": return AppendPath(GetBrainStatic(brainType, "Google"), "/chat/completions");
                    case "nvidia": return AppendPath(GetBrainStatic(brainType, "Nvidia"), "/v1/chat/completions");
                    case "openrouter": return AppendPath(GetBrainStatic(brainType, "OpenRouter"), "/chat/completions");
                    default:
                        string host = ReadStringMember(remote, "Host");
                        return string.IsNullOrEmpty(host) ? null : host;
                }
            }

            string server = ReadStringMember(brainInstance, "Server");
            if (!string.IsNullOrWhiteSpace(server)) return server;

            object local = GetInstanceField(type, brainInstance, "Local");
            if (local != null)
            {
                object portValue = GetInstanceField(local.GetType(), local, "port");
                int port = portValue is int ? (int)portValue : 0;
                return port > 0 ? "http://127.0.0.1:" + port : "http://127.0.0.1";
            }

            string ollama = brainType == null ? null : GetStaticField(brainType, "Ollama") as string;
            return string.IsNullOrWhiteSpace(ollama) ? "http://127.0.0.1:11434" : ollama;
        }

        public static string PrepareOutgoingMessages(object messageCollection, bool redact,
            out List<string> violations)
        {
            violations = new List<string>();
            IEnumerable messages = messageCollection as IEnumerable;
            if (messages == null) return string.Empty;

            StringBuilder preview = new StringBuilder();
            foreach (object message in messages)
            {
                if (message == null) continue;

                string role = ReadStringMember(message, "Role") ?? "message";
                string content = ReadStringMember(message, "Content");
                string thinking = ReadStringMember(message, "Thinking");
                string safeContent = InspectAndRedact(content, redact, violations);
                string safeThinking = InspectAndRedact(thinking, redact, violations);
                if (!string.IsNullOrEmpty(safeContent))
                    preview.AppendLine("[" + role.ToUpperInvariant() + "] " + safeContent);
                if (!string.IsNullOrEmpty(safeThinking))
                    preview.AppendLine("[THINKING] " + safeThinking);

                object calls = ReadMemberValue(message, "Calls");
                if (calls != null)
                {
                    InspectObject(calls, redact, violations, 0, new HashSet<object>(ReferenceComparer.Instance));
                    preview.AppendLine("[TOOL CALLS]");
                    AppendObjectPreview(calls, preview, 0);
                }

                object images = ReadMemberValue(message, "Images");
                int imageCount = CountItems(images);
                if (imageCount > 0) preview.AppendLine("[Images jointes : " + imageCount + "]");
            }

            const int previewLimit = 12000;
            if (preview.Length > previewLimit)
                return preview.ToString(0, previewLimit) + "\r\n[Contexte tronqué pour l'affichage]";
            return preview.ToString();
        }

        public static object CloneOutgoingMessages(object messageCollection)
        {
            IList messages = messageCollection as IList;
            if (messages == null)
                throw new InvalidOperationException("Outgoing Capri messages are not a mutable list.");

            IList copy = Activator.CreateInstance(messageCollection.GetType()) as IList;
            if (copy == null)
                throw new InvalidOperationException("Outgoing Capri messages could not be cloned safely.");
            Dictionary<object, object> clones = new Dictionary<object, object>(ReferenceComparer.Instance);
            foreach (object message in messages)
                copy.Add(CloneOutgoingObject(message, clones));
            return copy;
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

        private static object GetStaticField(Type type, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            return field == null ? null : field.GetValue(null);
        }

        private static string GetBrainStatic(Type brainType, string fieldName)
        {
            return brainType == null ? null : GetStaticField(brainType, fieldName) as string;
        }

        private static string AppendPath(string root, string path)
        {
            return string.IsNullOrWhiteSpace(root) ? null : root.TrimEnd('/') + path;
        }

        private static object GetInstanceField(Type type, object instance, string name)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return field == null ? null : field.GetValue(instance);
        }

        private static bool ClearStaticCollection(Type brainType, string collectionName, string gateName)
        {
            object collection = GetStaticField(brainType, collectionName);
            if (collection == null) return false;
            object gate = string.IsNullOrEmpty(gateName) ? collection : GetStaticField(brainType, gateName);
            lock (gate ?? collection)
            {
                MethodInfo clear = collection.GetType().GetMethod("Clear", Type.EmptyTypes);
                if (clear == null) return false;
                clear.Invoke(collection, null);
                return true;
            }
        }

        private static string ReadStringMember(object instance, string name)
        {
            if (instance == null) return null;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(instance) as string;
            PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(instance, null) as string;
        }

        private static void WriteStringMember(object instance, string name, string value)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(instance, value);
                return;
            }

            PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite) property.SetValue(instance, value, null);
        }

        private static string InspectAndRedact(string value, bool redact, List<string> violations)
        {
            if (string.IsNullOrEmpty(value)) return value;
            foreach (string violation in DlpEngine.Scan(value))
                if (!violations.Contains(violation)) violations.Add(violation);
            return redact ? DlpEngine.Redact(value) : value;
        }

        private static void InspectObject(object value, bool redact, List<string> violations, int depth, HashSet<object> visited)
        {
            if (value == null || depth > 4 || value is string || value.GetType().IsPrimitive || value.GetType().IsEnum)
                return;
            if (!value.GetType().IsValueType && !visited.Add(value)) return;

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                    InspectObject(item, redact, violations, depth + 1, visited);
                return;
            }

            FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name == "Images" || field.IsInitOnly) continue;
                object fieldValue = field.GetValue(value);
                string text = fieldValue as string;
                if (text != null)
                {
                    string safe = InspectAndRedact(text, redact, violations);
                    if (redact) field.SetValue(value, safe);
                }
                else
                {
                    InspectObject(fieldValue, redact, violations, depth + 1, visited);
                }
            }

        }

        private static object CloneOutgoingObject(object value, Dictionary<object, object> clones)
        {
            if (value == null || value is string || value.GetType().IsPrimitive || value.GetType().IsEnum || value.GetType().IsValueType)
                return value;
            object existing;
            if (clones.TryGetValue(value, out existing)) return existing;

            Array array = value as Array;
            if (array != null)
            {
                Array copy = (Array)array.Clone();
                clones.Add(value, copy);
                for (int i = 0; i < array.Length; i++)
                    copy.SetValue(CloneOutgoingObject(array.GetValue(i), clones), i);
                return copy;
            }

            IDictionary dictionary = value as IDictionary;
            if (dictionary != null)
            {
                IDictionary copy = Activator.CreateInstance(value.GetType()) as IDictionary;
                if (copy == null)
                    throw new InvalidOperationException("Outgoing tool arguments could not be cloned safely.");
                clones.Add(value, copy);
                foreach (DictionaryEntry item in dictionary)
                    copy.Add(CloneOutgoingObject(item.Key, clones), CloneOutgoingObject(item.Value, clones));
                return copy;
            }

            IList list = value as IList;
            if (list != null)
            {
                IList copy = Activator.CreateInstance(value.GetType()) as IList;
                if (copy == null)
                    throw new InvalidOperationException("Outgoing tool arguments could not be cloned safely.");
                clones.Add(value, copy);
                foreach (object item in list) copy.Add(CloneOutgoingObject(item, clones));
                return copy;
            }

            MethodInfo memberwiseClone = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            object clone = memberwiseClone.Invoke(value, null);
            clones.Add(value, clone);
            foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.IsInitOnly || field.Name == "Images") continue;
                field.SetValue(clone, CloneOutgoingObject(field.GetValue(value), clones));
            }
            return clone;
        }

        private static object ReadMemberValue(object instance, string name)
        {
            if (instance == null) return null;
            Type type = instance.GetType();
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(instance);
            PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(instance, null);
        }

        private static int CountItems(object value)
        {
            if (value == null) return 0;
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null) return 1;
            int count = 0;
            foreach (object item in enumerable) if (item != null) count++;
            return count;
        }

        private static void AppendObjectPreview(object value, StringBuilder preview, int depth)
        {
            if (value == null || depth > 4 || preview.Length >= 12000) return;
            if (value is string || value.GetType().IsPrimitive || value.GetType().IsEnum)
            {
                preview.AppendLine(Convert.ToString(value));
                return;
            }

            IDictionary dictionary = value as IDictionary;
            if (dictionary != null)
            {
                foreach (DictionaryEntry item in dictionary)
                {
                    preview.Append(Convert.ToString(item.Key)).Append(": ");
                    AppendObjectPreview(item.Value, preview, depth + 1);
                }
                return;
            }

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (object item in enumerable) AppendObjectPreview(item, preview, depth + 1);
                return;
            }

            FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.Name == "Images") continue;
                preview.Append(field.Name).Append(": ");
                AppendObjectPreview(field.GetValue(value), preview, depth + 1);
                if (preview.Length >= 12000) return;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object left, object right) { return ReferenceEquals(left, right); }
            public int GetHashCode(object value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
