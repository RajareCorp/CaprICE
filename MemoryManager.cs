using System;
using System.IO;
using System.Text;

namespace CaprICE
{
    public static class MemoryManager
    {
        public static string GetMemoryFilePath()
        {
            string localCapri = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri");
            
            string candidateMem = Path.Combine(localCapri, "memories.md");
            if (File.Exists(candidateMem)) return candidateMem;

            string candidateMe = Path.Combine(localCapri, "me.md");
            if (File.Exists(candidateMe)) return candidateMe;

            return candidateMem;
        }

        public static string ReadMemory()
        {
            string path = GetMemoryFilePath();
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
        }

        public static void SaveMemory(string content)
        {
            string path = GetMemoryFilePath();
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}