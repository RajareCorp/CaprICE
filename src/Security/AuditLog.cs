using System;
using System.IO;
using System.Text;

namespace CaprICE.Security
{
    /// <summary>Écriture thread-safe du journal d'audit texte.</summary>
    public static class AuditLog
    {
        private static readonly object LogLock = new object();

        public static string Path { get { return StoragePaths.AuditLog; } }

        public static void Write(InterceptEvent ev)
        {
            try
            {
                lock (LogLock)
                {
                    string line = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss} | {1,-9} | {2,-22} | {3,-10} | {4,5} ms | {5} | {6}",
                        ev.Timestamp,
                        ev.Status,
                        ev.TargetName,
                        ev.Method,
                        ev.LatencyMs < 0 ? "-" : ev.LatencyMs.ToString(),
                        ev.Url,
                        ev.Reason
                    );

                    File.AppendAllText(Path, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}
