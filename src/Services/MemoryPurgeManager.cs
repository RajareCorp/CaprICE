using System;
using System.Collections.Generic;
using System.IO;
using CaprICE.Hooks;

namespace CaprICE.Services
{
    public sealed class MemoryPurgeResult
    {
        public string PreservedVaultNote { get; internal set; }
        public List<string> DeletedPaths { get; private set; }
        public List<string> Errors { get; private set; }

        internal MemoryPurgeResult()
        {
            DeletedPaths = new List<string>();
            Errors = new List<string>();
        }
    }

    public static class MemoryPurgeManager
    {
        public static string GetVaultNoteToPreserve()
        {
            string activePath = CaprICEHook.GetActiveMemoryPath();
            if (string.IsNullOrWhiteSpace(activePath)) return null;

            string local = CapriDataPaths.DirectoryPath;
            if (PathsEqual(activePath, Path.Combine(local, "me.md"))
                || PathsEqual(activePath, Path.Combine(local, "memories.md")))
                return null;
            return activePath;
        }

        public static MemoryPurgeResult PurgeLocalData()
        {
            MemoryPurgeResult result = new MemoryPurgeResult
            {
                PreservedVaultNote = GetVaultNoteToPreserve()
            };

            string local = CapriDataPaths.DirectoryPath;
            TryDelete(Path.Combine(local, "chat.json"), result);
            TryDelete(Path.Combine(local, "me.md"), result);
            TryDelete(Path.Combine(local, "memories.md"), result);

            string transcripts = Path.Combine(local, "transcripts");
            try
            {
                if (Directory.Exists(transcripts))
                {
                    foreach (string path in Directory.GetFiles(transcripts, "*.md", SearchOption.TopDirectoryOnly))
                        TryDelete(path, result);
                    TryDelete(Path.Combine(transcripts, "unfolded.txt"), result);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add("Unable to enumerate transcripts: " + ex.Message);
            }

            try
            {
                if (!CaprICEHook.ClearBrainHistory())
                    result.Errors.Add("Capri's in-memory history could not be located.");
            }
            catch (Exception ex)
            {
                result.Errors.Add("Unable to clear Capri's in-memory state: " + ex.Message);
            }

            return result;
        }

        private static void TryDelete(string path, MemoryPurgeResult result)
        {
            try
            {
                if (!File.Exists(path)) return;
                File.Delete(path);
                result.DeletedPaths.Add(path);
            }
            catch (Exception ex)
            {
                result.Errors.Add(path + " : " + ex.Message);
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return false;
            return string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
