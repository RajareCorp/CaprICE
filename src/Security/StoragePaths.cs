using System;
using System.IO;

namespace CaprICE.Security
{
    /// <summary>Emplacements des fichiers CaprICE : %LocalAppData%\Capri\CaprICE\.</summary>
    internal static class StoragePaths
    {
        public static string Directory
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri", "CaprICE");
                if (!System.IO.Directory.Exists(dir))
                {
                    try { System.IO.Directory.CreateDirectory(dir); } catch { return AppDomain.CurrentDomain.BaseDirectory; }
                }
                return dir;
            }
        }

        public static string AuditLog { get { return Path.Combine(Directory, "caprice_audit.jsonl"); } }
        public static string Settings { get { return Path.Combine(Directory, "caprice_settings.json"); } }
    }
}
