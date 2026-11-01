using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CaprICE.Services
{
    public sealed class TranscriptData
    {
        public string Name { get; private set; }
        public string Content { get; private set; }

        internal TranscriptData(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }

    public static class TranscriptManager
    {
        private static string TranscriptDirectory
        {
            get { return Path.Combine(CapriDataPaths.DirectoryPath, "transcripts"); }
        }

        public static List<TranscriptData> LoadAll()
        {
            List<TranscriptData> transcripts = new List<TranscriptData>();
            string directory = TranscriptDirectory;
            if (!Directory.Exists(directory)) return transcripts;

            string[] dailyFiles = Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly);
            System.Array.Sort(dailyFiles, System.StringComparer.OrdinalIgnoreCase);
            System.Array.Reverse(dailyFiles);

            string unfoldedPath = Path.Combine(directory, "unfolded.txt");
            if (File.Exists(unfoldedPath))
            {
                transcripts.Add(new TranscriptData("unfolded.txt", File.ReadAllText(unfoldedPath, Encoding.UTF8)));
            }

            foreach (string path in dailyFiles)
            {
                transcripts.Add(new TranscriptData(Path.GetFileName(path), File.ReadAllText(path, Encoding.UTF8)));
            }

            return transcripts;
        }

        public static void Delete(string fileName)
        {
            string path = GetTranscriptPath(fileName);
            File.Delete(path);
        }

        public static void DeleteAll()
        {
            string directory = TranscriptDirectory;
            if (!Directory.Exists(directory)) return;

            foreach (string path in Directory.GetFiles(directory, "*.md", SearchOption.TopDirectoryOnly))
            {
                File.Delete(path);
            }

            string unfoldedPath = Path.Combine(directory, "unfolded.txt");
            if (File.Exists(unfoldedPath)) File.Delete(unfoldedPath);
        }

        private static string GetTranscriptPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || Path.GetFileName(fileName) != fileName
                || (fileName != "unfolded.txt" && Path.GetExtension(fileName) != ".md"))
            {
                throw new IOException("Nom de transcription invalide.");
            }

            return Path.Combine(TranscriptDirectory, fileName);
        }
    }
}
