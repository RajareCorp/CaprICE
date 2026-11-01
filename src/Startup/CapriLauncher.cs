using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CaprICE.Startup
{
    /// <summary>Charge l'assembly Capri en mémoire et l'exécute dans un thread STA d'arrière-plan.</summary>
    internal static class CapriLauncher
    {
        private static Thread capriThread;

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
            capriThread = new Thread(delegate()
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
                catch (ThreadAbortException) 
                {
                    // Catch attendu si le thread est brusquement interrompu
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[CaprICE Runtime] Erreur thread Capri: " + ex.Message);
                }
            });
            capriThread.SetApartmentState(ApartmentState.STA);
            capriThread.IsBackground = true;
            capriThread.Start();
        }

        /// <summary>
        /// Ferme proprement la boucle de messages WinForms et les fenêtres créées sur le thread Capri.
        /// </summary>
        public static void Stop()
        {
            if (capriThread == null || !capriThread.IsAlive) return;

            try
            {
                // Parcourt toutes les fenêtres ouvertes dans l'application
                FormCollection openForms = Application.OpenForms;
                for (int i = openForms.Count - 1; i >= 0; i--)
                {
                    Form form = openForms[i];

                    // Si la fenêtre appartient au thread Capri (et non à CaprICEForm)
                    if (form != null && !form.IsDisposed && form.InvokeRequired)
                    {
                        try
                        {
                            form.BeginInvoke((Action)delegate
                            {
                                form.Close();
                                form.Dispose();
                            });
                        }
                        catch { }
                    }
                }

                // Attend la fermeture naturelle du thread Capri pendant 1,5 seconde
                if (!capriThread.Join(1500))
                {
                    Debug.WriteLine("[CapriLauncher] Capri n'a pas quitté dans les temps, fermeture forcée de l'application.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[CapriLauncher] Exception lors de l'arrêt propre : " + ex.Message);
            }
        }
    }
}