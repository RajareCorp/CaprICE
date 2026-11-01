using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CaprICE.Startup
{
    /// <summary>Charge l'assembly Capri en mémoire et l'exécute dans un thread STA d'arrière-plan.</summary>
    internal static class CapriLauncher
    {
        /// <summary>
        /// CRITIQUE : le dossier courant doit être celui de Capri pour que settings.json,
        /// memories.md, chat.json et les packs vocaux soient lus depuis l'installation officielle.
        /// </summary>
        public static void UseCapriDirectory(string capriPath)
        {
            string capriDir = Path.GetDirectoryName(capriPath);
            if (!string.IsNullOrEmpty(capriDir) && Directory.Exists(capriDir))
            {
                Directory.SetCurrentDirectory(capriDir);
            }
        }

        public static Assembly Load(string capriPath)
        {
            try
            {
                return Assembly.LoadFile(Path.GetFullPath(capriPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de charger l'assembly Capri :\n" + ex.Message, "CaprICE Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public static void StartInBackground(Assembly capriAssembly, string[] args)
        {
            Thread capriThread = new Thread(delegate()
            {
                try
                {
                    MethodInfo entry = capriAssembly.EntryPoint;
                    if (entry != null)
                    {
                        object[] entryArgs = entry.GetParameters().Length == 0 ? null : new object[] { args };
                        entry.Invoke(null, entryArgs);
                    }
                    else
                    {
                        Type prog = capriAssembly.GetType("CapriDesktop.Program");
                        if (prog != null)
                        {
                            MethodInfo main = prog.GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
                            if (main != null) main.Invoke(null, new object[] { args });
                        }
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    Console.WriteLine("[CaprICE Runtime] Erreur thread Capri: " + ex.Message);
                }
            });
            capriThread.SetApartmentState(ApartmentState.STA);
            capriThread.IsBackground = true;
            capriThread.Start();
        }
    }
}
