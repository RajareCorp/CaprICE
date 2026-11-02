using System;
using System.IO;
using System.Windows.Forms;

namespace CaprICE.Startup
{
    /// <summary>Retrouve Capri.exe : argument --capri, installation officielle, puis candidats locaux.</summary>
    internal static class CapriLocator
    {
        public static string Resolve(string[] args)
        {
            // 1. Argument explicite --capri <chemin>
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--capri") return args[i + 1];
            }

            // 2. Dossier d'installation officiel : %LocalAppData%\Capri\Capri.exe
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string installedCapri = Path.Combine(localAppData, "Capri", "Capri.exe");
            if (File.Exists(installedCapri))
            {
                return installedCapri;
            }

            // 3. Candidats de repli (builds locaux ou côte à côte)
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

        /// <summary>Demande à l'utilisateur de désigner Capri.exe. Retourne null s'il annule.</summary>
        public static string AskUser()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "CaprICE // Select Capri.exe";
                ofd.Filter = "Capri executable (Capri.exe)|Capri.exe|All executables (*.exe)|*.exe";
                string defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capri");
                if (Directory.Exists(defaultDir)) ofd.InitialDirectory = defaultDir;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName;
                }

                MessageBox.Show("CaprICE requires Capri.exe to run.", "CaprICE",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }
    }
}
