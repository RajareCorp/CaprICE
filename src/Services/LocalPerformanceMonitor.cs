using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CaprICE.Services
{
    internal static class LocalPerformanceMonitor
    {
        private const uint PdhMoreData = 0x800007D2;
        private const uint PdhFormatDouble = 0x00000200;
        private const uint PdhValidData = 0;
        private const uint PdhNewData = 1;
        private const string GpuCounterPath = @"\GPU Engine(*)\Utilization Percentage";

        public static string GetReport()
        {
            StringBuilder report = new StringBuilder("État des performances (mesures locales) :");
            try
            {
                report.AppendLine();
                report.Append("CPU : ").Append(ReadCpuUsage().ToString("0", CultureInfo.InvariantCulture)).Append("%");
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine("Unable to read CPU usage: " + ex);
                report.AppendLine().Append("CPU : indisponible (").Append(ex.Message).Append(')');
            }

            try
            {
                MemoryStatus memory = ReadMemoryStatus();
                ulong used = memory.TotalPhysical - memory.AvailablePhysical;
                double percent = memory.TotalPhysical == 0 ? 0 : used * 100.0 / memory.TotalPhysical;
                report.AppendLine().Append("RAM : ")
                    .Append(ToGigabytes(used)).Append(" / ").Append(ToGigabytes(memory.TotalPhysical))
                    .Append(" Go (").Append(percent.ToString("0", CultureInfo.InvariantCulture)).Append("%)");
            }
            catch (Win32Exception ex)
            {
                Debug.WriteLine("Unable to read RAM usage: " + ex);
                report.AppendLine().Append("RAM : indisponible (").Append(ex.Message).Append(')');
            }

            report.AppendLine().Append("GPU 3D : ").Append(ReadGpuUsage());
            report.AppendLine().Append("Ces mesures restent sur cet ordinateur.");
            return report.ToString();
        }

        private static double ReadCpuUsage()
        {
            SystemTimes before = GetSystemTimes();
            Thread.Sleep(350);
            SystemTimes after = GetSystemTimes();
            ulong idle = after.Idle - before.Idle;
            ulong total = after.Kernel - before.Kernel + after.User - before.User;
            if (total == 0) throw new Win32Exception("Aucune mesure CPU n'a été fournie.");
            return Math.Max(0, Math.Min(100, (1.0 - (double)idle / total) * 100.0));
        }

        private static SystemTimes GetSystemTimes()
        {
            NativeFileTime idle;
            NativeFileTime kernel;
            NativeFileTime user;
            if (!GetSystemTimes(out idle, out kernel, out user))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return new SystemTimes
            {
                Idle = idle.ToUInt64(),
                Kernel = kernel.ToUInt64(),
                User = user.ToUInt64()
            };
        }

        private static MemoryStatus ReadMemoryStatus()
        {
            MemoryStatus status = new MemoryStatus { Length = (uint)Marshal.SizeOf(typeof(MemoryStatus)) };
            if (!GlobalMemoryStatusEx(ref status))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return status;
        }

        private static string ReadGpuUsage()
        {
            uint bufferLength = 0;
            uint status = PdhExpandWildCardPathW(null, GpuCounterPath, null, ref bufferLength, 0);
            if (status != PdhMoreData || bufferLength == 0)
                return "indisponible (compteurs GPU Windows absents)";

            StringBuilder paths = new StringBuilder((int)bufferLength);
            status = PdhExpandWildCardPathW(null, GpuCounterPath, paths, ref bufferLength, 0);
            if (status != 0)
                return "indisponible (lecture des compteurs Windows impossible)";

            IntPtr query;
            status = PdhOpenQueryW(null, UIntPtr.Zero, out query);
            if (status != 0)
                return "indisponible (compteurs Windows, code " + status.ToString("X8") + ")";

            List<IntPtr> counters = new List<IntPtr>();
            try
            {
                foreach (string path in paths.ToString().Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (path.IndexOf("engtype_3D", StringComparison.OrdinalIgnoreCase) < 0) continue;

                    IntPtr counter;
                    if (PdhAddEnglishCounterW(query, path, UIntPtr.Zero, out counter) == 0)
                        counters.Add(counter);
                }

                if (counters.Count == 0)
                    return "indisponible (aucun moteur 3D détecté)";
                if (PdhCollectQueryData(query) != 0)
                    return "indisponible (lecture des compteurs GPU impossible)";
                Thread.Sleep(350);
                if (PdhCollectQueryData(query) != 0)
                    return "indisponible (lecture des compteurs GPU impossible)";

                double total = 0;
                foreach (IntPtr counter in counters)
                {
                    uint type;
                    FormattedCounterValue value;
                    if (PdhGetFormattedCounterValue(counter, PdhFormatDouble, out type, out value) == 0
                        && (value.Status == PdhValidData || value.Status == PdhNewData)
                        && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value))
                        total += Math.Max(0, value.Value);
                }

                return Math.Min(100, total).ToString("0", CultureInfo.InvariantCulture) + "%";
            }
            finally
            {
                PdhCloseQuery(query);
            }
        }

        private static string ToGigabytes(ulong bytes)
        {
            return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.0", CultureInfo.InvariantCulture);
        }

        private struct SystemTimes
        {
            public ulong Idle;
            public ulong Kernel;
            public ulong User;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeFileTime
        {
            public uint Low;
            public uint High;

            public ulong ToUInt64()
            {
                return ((ulong)High << 32) | Low;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MemoryStatus
        {
            public uint Length;
            public uint MemoryLoad;
            public ulong TotalPhysical;
            public ulong AvailablePhysical;
            public ulong TotalPageFile;
            public ulong AvailablePageFile;
            public ulong TotalVirtual;
            public ulong AvailableVirtual;
            public ulong AvailableExtendedVirtual;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FormattedCounterValue
        {
            public uint Status;
            public double Value;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out NativeFileTime idleTime, out NativeFileTime kernelTime, out NativeFileTime userTime);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool GlobalMemoryStatusEx(ref MemoryStatus buffer);

        [DllImport("pdh.dll", EntryPoint = "PdhExpandWildCardPathW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint PdhExpandWildCardPathW(string dataSource, string path, StringBuilder buffer, ref uint bufferLength, uint flags);

        [DllImport("pdh.dll", EntryPoint = "PdhOpenQueryW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint PdhOpenQueryW(string dataSource, UIntPtr userData, out IntPtr query);

        [DllImport("pdh.dll", EntryPoint = "PdhAddEnglishCounterW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint PdhAddEnglishCounterW(IntPtr query, string path, UIntPtr userData, out IntPtr counter);

        [DllImport("pdh.dll")]
        private static extern uint PdhCollectQueryData(IntPtr query);

        [DllImport("pdh.dll")]
        private static extern uint PdhGetFormattedCounterValue(IntPtr counter, uint format, out uint type, out FormattedCounterValue value);

        [DllImport("pdh.dll")]
        private static extern uint PdhCloseQuery(IntPtr query);
    }
}
