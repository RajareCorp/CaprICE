using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace CaprICE
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string capriPath = ResolveCapriPath(args);
            if (string.IsNullOrEmpty(capriPath) || !File.Exists(capriPath))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = "CaprICE // Sélectionner le binaire Capri.exe";
                    ofd.Filter = "Exécutable Capri (Capri.exe)|Capri.exe|Tous les exécutables (*.exe)|*.exe";
                    string defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri");
                    if (Directory.Exists(defaultDir)) ofd.InitialDirectory = defaultDir;

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        capriPath = ofd.FileName;
                    }
                    else
                    {
                        MessageBox.Show("CaprICE a besoin de Capri.exe pour fonctionner.", "CaprICE",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            // CRITICAL: Set working directory to Capri's install directory so settings.json,
            // memories.md, chat.json and voice packs are loaded from the official installation.
            string capriDir = Path.GetDirectoryName(capriPath);
            if (!string.IsNullOrEmpty(capriDir) && Directory.Exists(capriDir))
            {
                Directory.SetCurrentDirectory(capriDir);
            }

            Assembly capriAssembly;
            try
            {
                capriAssembly = Assembly.LoadFile(Path.GetFullPath(capriPath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de charger l'assembly Capri :\n" + ex.Message, "CaprICE Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 1. Install In-Memory Harmony Hooks
            CaprICEHook.Initialize(capriAssembly);

            // 2. Launch Capri in background STA thread
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

            // 3. Run CaprICE Mission Control UI
            Application.Run(new CaprICEForm());
        }

        private static string ResolveCapriPath(string[] args)
        {
            // 1. Explicit argument --capri <path>
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--capri") return args[i + 1];
            }

            // 2. Official Windows installation folder: %LocalAppData%\Capri\Capri.exe
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installedCapri = Path.Combine(localAppData, "Capri", "Capri.exe");
            if (File.Exists(installedCapri))
            {
                return installedCapri;
            }

            // 3. Fallback candidates (local builds or side-by-side)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates = new string[]
            {
                Path.Combine(baseDir, "Capri.exe"),
                Path.Combine(baseDir, "..", "out", "windows", "Capri.exe"),
                Path.Combine(baseDir, "..", "Capri.exe"),
                Path.GetFullPath(@"out\windows\Capri.exe")
            };

            foreach (string candidate in candidates)
            {
                try
                {
                    if (File.Exists(candidate)) return candidate;
                }
                catch { }
            }

            return null;
        }
    }
}
