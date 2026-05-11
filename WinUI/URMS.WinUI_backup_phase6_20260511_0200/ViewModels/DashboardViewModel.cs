using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using URMS.WinUI.Services;

namespace URMS.WinUI.ViewModels
{
    /// <summary>
    /// ダッシュボード全体の状態を保持する ViewModel。
    /// PropertyChanged はすべて DispatcherQueueTimer 経由（UI スレッド）で発火する。
    /// </summary>
    public class DashboardViewModel : ObservableObject, IDisposable
    {
        private readonly SystemMonitorService  _sysmon = new();
        private readonly NetworkMonitorService _netmon = new();
        private readonly CiCdService           _cicd   = new();
        private DispatcherQueueTimer?          _timer;
        private bool                           _disposed;

        // ── Session ──────────────────────────────────────────
        public string SessionId { get; } = "URX-" + Guid.NewGuid().ToString("N")[..5].ToUpper();

        // ── CPU / RAM / GPU ──────────────────────────────────
        private double _cpuUsage;
        public double CpuUsage { get => _cpuUsage; private set => SetProperty(ref _cpuUsage, value); }

        private double _ramUsage;
        public double RamUsage { get => _ramUsage; private set => SetProperty(ref _ramUsage, value); }

        private double _gpuUsage;
        public double GpuUsage { get => _gpuUsage; private set => SetProperty(ref _gpuUsage, value); }

        // ── Disk ──────────────────────────────────────────────
        private double _diskC;
        public double DiskC { get => _diskC; private set => SetProperty(ref _diskC, value); }

        private string _diskCText = "—";
        public string DiskCText { get => _diskCText; private set => SetProperty(ref _diskCText, value); }

        private double _diskD;
        public double DiskD { get => _diskD; private set => SetProperty(ref _diskD, value); }

        private string _diskDText = "—";
        public string DiskDText { get => _diskDText; private set => SetProperty(ref _diskDText, value); }

        // ── Network ───────────────────────────────────────────
        private string _netLatency = "—";
        public string NetLatency { get => _netLatency; private set => SetProperty(ref _netLatency, value); }

        private string _netStatus = "—";
        public string NetStatus { get => _netStatus; private set => SetProperty(ref _netStatus, value); }

        // ── CI/CD ─────────────────────────────────────────────
        private string _ciCdStatus = "—";
        public string CiCdStatus { get => _ciCdStatus; private set => SetProperty(ref _ciCdStatus, value); }

        // ── 起動 / 停止 ───────────────────────────────────────
        public void StartRefresh(DispatcherQueue queue)
        {
            _timer = queue.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += async (_, _) => await RefreshAsync();
            _timer.Start();
        }

        public void StopRefresh()
        {
            _timer?.Stop();
            _timer = null;
        }

        /// <summary>実データ取得 → プロパティ更新（UI スレッドで呼ばれる）</summary>
        public async Task RefreshAsync()
        {
            // バックグラウンドで並列取得
            var metricsTask = _sysmon.GetMetricsAsync();
            var netTask     = _netmon.GetStatusAsync();
            var ciTask      = _cicd.GetLatestStatusAsync();

            var m = await metricsTask;
            CpuUsage  = m.CpuPercent;
            RamUsage  = m.RamPercent;
            GpuUsage  = m.GpuPercent;
            DiskC     = m.DiskCPercent;
            DiskCText = $"{m.DiskCPercent:F0}%";
            DiskD     = m.DiskDPercent;
            DiskDText = $"{m.DiskDPercent:F0}%";

            var net   = await netTask;
            NetLatency = net.IsUp ? $"{net.LatencyMs}ms" : "—";
            NetStatus  = net.IsUp ? "稼働中" : "切断";

            CiCdStatus = await ciTask;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopRefresh();
            _sysmon.Dispose();
        }
    }
}
