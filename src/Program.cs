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

            // 1. Hooks Harmony en mémoire
            CaprICEHook.Initialize(capriAssembly);

            // 2. Capri dans un thread STA d'arrière-plan
            CapriLauncher.StartInBackground(capriAssembly, args);

            // 3. Interface CaprICE
            Application.Run(new CaprICEForm());
        }
    }
}
