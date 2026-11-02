using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CaprICE.Security
{
    public sealed class AppLaunchAction
    {
        public string Keyword { get; private set; }
        public string FilePath { get; private set; }

        public AppLaunchAction(string keyword, string filePath)
        {
            Keyword = keyword;
            FilePath = filePath;
        }

        public override string ToString()
        {
            return Keyword + "  →  " + Path.GetFileName(FilePath);
        }
    }

    internal static class AppLaunchActions
    {
        private static readonly List<AppLaunchAction> actions = new List<AppLaunchAction>();
        private static readonly object syncRoot = new object();

        public static IList<AppLaunchAction> GetAll()
        {
            lock (syncRoot)
                return new List<AppLaunchAction>(actions);
        }

        public static void ReplaceAll(IEnumerable<AppLaunchAction> replacements)
        {
            List<AppLaunchAction> validated = new List<AppLaunchAction>();
            HashSet<string> keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (AppLaunchAction action in replacements)
            {
                Validate(action.Keyword, action.FilePath, false);
                if (!keywords.Add(action.Keyword))
                    throw new ArgumentException("Chaque mot-clé doit être unique.");
                validated.Add(action);
            }

            lock (syncRoot)
            {
                actions.Clear();
                actions.AddRange(validated);
            }
        }

        public static string Serialize()
        {
            StringBuilder serialized = new StringBuilder();
            foreach (AppLaunchAction action in GetAll())
            {
                if (serialized.Length > 0) serialized.Append('\n');
                serialized.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(action.Keyword)));
                serialized.Append('\t');
                serialized.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(action.FilePath)));
            }
            return serialized.ToString();
        }

        public static void Load(string serialized)
        {
            List<AppLaunchAction> loaded = new List<AppLaunchAction>();
            if (!string.IsNullOrEmpty(serialized))
            {
                string[] lines = serialized.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length != 2)
                        throw new FormatException("Une association de lancement est mal formée.");

                    string keyword = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                    string executablePath = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                    Validate(keyword, executablePath, false);
                    loaded.Add(new AppLaunchAction(keyword, executablePath));
                }
            }
            ReplaceAll(loaded);
        }

        public static bool TryExecuteExact(string text, out string reply)
        {
            string keyword = NormalizeCommand(text);
            foreach (AppLaunchAction action in GetAll())
            {
                if (!string.Equals(keyword, action.Keyword, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!File.Exists(action.FilePath))
                {
                    reply = "Impossible d'ouvrir " + Path.GetFileName(action.FilePath)
                        + " : le fichier configuré est introuvable.";
                    return true;
                }

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = action.FilePath,
                        WorkingDirectory = Path.GetDirectoryName(action.FilePath),
                        UseShellExecute = true
                    });
                    reply = "J'ouvre " + Path.GetFileName(action.FilePath) + ".";
                }
                catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException || ex is IOException)
                {
                    Debug.WriteLine("Unable to run configured application: " + ex);
                    reply = "L'ouverture de " + Path.GetFileName(action.FilePath)
                        + " a échoué : " + ex.Message;
                }
                return true;
            }

            reply = null;
            return false;
        }

        public static string NormalizeCommand(string text)
        {
            return (text ?? string.Empty).Trim().TrimEnd('.', '!', '?', ',', ';').Trim();
        }

        public static void Validate(string keyword, string filePath, bool requireExistingFile)
        {
            keyword = NormalizeCommand(keyword);
            if (keyword.Length == 0 || keyword.Length > 64
                || keyword.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new ArgumentException("Le mot-clé doit être un seul mot de 1 à 64 caractères.");
            if (string.Equals(keyword, "Performance", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Le mot-clé « Performance » est réservé.");
            if (string.IsNullOrWhiteSpace(filePath) || !Path.IsPathRooted(filePath))
                throw new ArgumentException("Sélectionnez un chemin de fichier complet.");
            if (requireExistingFile && !File.Exists(filePath))
                throw new FileNotFoundException("Le fichier sélectionné est introuvable.", filePath);
        }
    }
}
