using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace URMS.WinUI.Services
{
    public record SystemMetrics(
        double CpuPercent,
        double RamPercent,
        double GpuPercent,
        double DiskCPercent,
        double DiskDPercent,
        double DiskNasPercent);

    /// <summary>CPU / RAM / Disk 実測値を提供するサービス（追加NuGet不要）</summary>
    public class SystemMonitorService : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME { public uint Low; public uint High; }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength, dwMemoryLoad;
            public ulong ullTotalPhys, ullAvailPhys;
            public ulong ullTotalPageFile, ullAvailPageFile;
            public ulong ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll")] static extern bool GetSystemTimes(out FILETIME idle, out FILETIME kernel, out FILETIME user);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX s);

        private FILETIME _pIdle, _pKernel, _pUser;
        private bool _firstCall = true;
        private bool _disposed;

        private static long ToLong(FILETIME ft) => ((long)ft.High << 32) | ft.Low;

        private double ReadCpu()
        {
            if (!GetSystemTimes(out var idle, out var kernel, out var user)) return 0;
            if (_firstCall) { _pIdle = idle; _pKernel = kernel; _pUser = user; _firstCall = false; return 0; }

            long dIdle   = ToLong(idle)   - ToLong(_pIdle);
            long dKernel = ToLong(kernel) - ToLong(_pKernel);
            long dUser   = ToLong(user)   - ToLong(_pUser);
            _pIdle = idle; _pKernel = kernel; _pUser = user;

            long total = dKernel + dUser;
            return total == 0 ? 0 : (1.0 - (double)dIdle / total) * 100.0;
        }

        private static double ReadRam()
        {
            var s = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            return GlobalMemoryStatusEx(ref s) ? s.dwMemoryLoad : 0;
        }

        private static double GetDrivePercent(string root)
        {
            try
            {
                var di = new DriveInfo(root);
                return di.IsReady ? (1.0 - (double)di.AvailableFreeSpace / di.TotalSize) * 100.0 : 0;
            }
            catch { return 0; }
        }

        public Task<SystemMetrics> GetMetricsAsync() => Task.Run(() => new SystemMetrics(
            CpuPercent:  Math.Round(ReadCpu(),           1),
            RamPercent:  Math.Round(ReadRam(),           1),
            GpuPercent:  0, // GPU: WMI 依存回避のため省略
            DiskCPercent: Math.Round(GetDrivePercent("C:"), 1),
            DiskDPercent: Math.Round(GetDrivePercent("D:"), 1),
            DiskNasPercent: 0));

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}
