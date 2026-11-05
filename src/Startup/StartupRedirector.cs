using System;
using System.IO;
using Microsoft.Win32;

namespace CaprICE.Startup
{
    /// <summary>Remplace l'entrée de démarrage Capri activée par CaprICE sans réactiver une entrée désactivée.</summary>
    internal static class StartupRedirector
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ApprovedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        private const string BackupKey = @"Software\CaprICE\Startup";
        private const string BackupValueName = "OriginalCapriCommand";
        private const string CapriValueName = "Capri";

        public static void ReplaceEnabledCapriStartup(string capriPath, string capricePath)
        {
            string expectedCapriPath = Path.GetFullPath(capriPath);
            string replacementPath = Path.GetFullPath(capricePath);

            using (RegistryKey run = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                if (run == null) return;

                string command = run.GetValue(CapriValueName) as string;
                string commandPath;
                string arguments;
                if (!TrySplitCommand(command, out commandPath, out arguments) ||
                    !PathsEqual(commandPath, expectedCapriPath))
                {
                    return;
                }

                using (RegistryKey approved = Registry.CurrentUser.OpenSubKey(ApprovedKey))
                {
                    byte[] state = approved == null ? null : approved.GetValue(CapriValueName) as byte[];
                    if (state != null && state.Length > 0 && (state[0] & 1) != 0) return;
                }

                using (RegistryKey backup = Registry.CurrentUser.CreateSubKey(BackupKey))
                {
                    if (backup.GetValue(BackupValueName) == null)
                    {
                        backup.SetValue(BackupValueName, command, RegistryValueKind.String);
                    }
                }

                run.SetValue(CapriValueName, "\"" + replacementPath + "\"" + arguments, RegistryValueKind.String);
            }
        }

        private static bool TrySplitCommand(string command, out string executable, out string arguments)
        {
            executable = null;
            arguments = null;
            if (string.IsNullOrWhiteSpace(command)) return false;

            string value = command.TrimStart();
            if (value[0] == '"')
            {
                int endQuote = value.IndexOf('"', 1);
                if (endQuote <= 1) return false;
                executable = value.Substring(1, endQuote - 1);
                arguments = value.Substring(endQuote + 1);
                return true;
            }

            int endPath = value.IndexOfAny(new[] { ' ', '\t' });
            executable = endPath < 0 ? value : value.Substring(0, endPath);
            arguments = endPath < 0 ? string.Empty : value.Substring(endPath);
            return true;
        }

        private static bool PathsEqual(string left, string right)
        {
            try
            {
                return string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException) { return false; }
            catch (NotSupportedException) { return false; }
            catch (PathTooLongException) { return false; }
        }
    }
}
