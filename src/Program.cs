using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CaprICE.Hooks;
using CaprICE.Startup;
using CaprICE.UI;

namespace CaprICE
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                NativeMethods.SetProcessDpiAwarenessContext(new IntPtr(-4));
            }
            catch (EntryPointNotFoundException)
            {
                NativeMethods.SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string capriPath = CapriLocator.Resolve(args);
            if (string.IsNullOrEmpty(capriPath) || !File.Exists(capriPath))
            {
                capriPath = CapriLocator.AskUser();
                if (capriPath == null) return;
            }

            CapriLauncher.UseCapriDirectory(capriPath);

            Assembly capriAssembly = CapriLauncher.Load(capriPath);
            if (capriAssembly == null) return;

            try
            {
                StartupRedirector.ReplaceEnabledCapriStartup(capriPath, Application.ExecutablePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowStartupWarning(ex);
            }
            catch (System.Security.SecurityException ex)
            {
                ShowStartupWarning(ex);
            }
            catch (IOException ex)
            {
                ShowStartupWarning(ex);
            }

            // 1. Hooks Harmony en mémoire
            CaprICEHook.Initialize(capriAssembly);

            // 2. Capri dans un thread STA d'arrière-plan
            CapriLauncher.StartInBackground(capriAssembly, args);

            // 3. Interface CaprICE
            Application.Run(new CaprICEForm());
        }

        private static void ShowStartupWarning(Exception ex)
        {
            MessageBox.Show("CaprICE couldn't replace Capri's Windows startup entry:\n" + ex.Message,
                "CaprICE Startup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
