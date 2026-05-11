using System;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using URMS.WinUI.Services;
using URMS.WinUI.ViewModels;

namespace URMS.WinUI.Pages
{
    public sealed partial class DashboardPage : Page
    {
        private readonly LanguageService    _lang = LanguageService.Instance;
        private readonly DashboardViewModel _vm   = new();
        private DispatcherQueueTimer?       _waveTimer;
        private DispatcherQueueTimer?       _operationTimer;
        private double                      _wavePhase;
        private int                         _workflowStep;

        public DashboardPage()
        {
            this.InitializeComponent();

            _vm.PropertyChanged += OnVmPropertyChanged;

            _lang.LanguageChanged              += (_, _) => Apply();
            ThemeService.Instance.ThemeChanged += (_, _) => Apply();

            this.Loaded   += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            App.RecordDashboardLoaded(); // フェーズ4: 起動時間計測
            Apply();
            InitNetWave();
            InitOperationLayer();
            _vm.StartRefresh(DispatcherQueue.GetForCurrentThread());
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _waveTimer?.Stop();
            _waveTimer = null;
            _operationTimer?.Stop();
            _operationTimer = null;
            _vm.StopRefresh();
            _vm.Dispose();
        }

        /// <summary>ViewModel プロパティ変更 → 対応 UI 要素を同期</summary>
        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DashboardViewModel.CpuUsage):
                    GaugeCpu.Value = _vm.CpuUsage; break;
                case nameof(DashboardViewModel.RamUsage):
                    GaugeRam.Value = _vm.RamUsage; break;
                case nameof(DashboardViewModel.GpuUsage):
                    GaugeGpu.Value = _vm.GpuUsage; break;
                case nameof(DashboardViewModel.DiskC):
                    BarDiskC.Value = _vm.DiskC; break;
                case nameof(DashboardViewModel.DiskCText):
                    TxtDiskC.Text  = _vm.DiskCText; break;
                case nameof(DashboardViewModel.DiskD):
                    BarDiskD.Value = _vm.DiskD; break;
                case nameof(DashboardViewModel.DiskDText):
                    TxtDiskD.Text  = _vm.DiskDText; break;
                case nameof(DashboardViewModel.NetLatency):
                    // NetWaveCanvas のラベルは波形アニメで更新、ここは将来拡張用
                    break;
                case nameof(DashboardViewModel.NetStatus):
                    // Subsystem layer カード更新は以降で処理
                    break;
            }
        }

        private void Apply()
        {
            var L = _lang;
            TxtSessionId.Text    = _vm.SessionId;
            TxtCardSchedule.Text = L.Get("CardSchedule");
            TxtCardWeather.Text  = L.Get("CardWeather");
            TxtCardTask.Text     = L.Get("CardTask");

            WxAnim.Condition    = "partly_cloudy";
            TxtWeatherTemp.Text = "22°C";
            TxtWeatherDesc.Text = "Tokyo · Minato";

            // 初期表示値（VM が未取得の間のフォールバック）
            GaugeCpu.Value = 0;
            GaugeRam.Value = 0;
            GaugeGpu.Value = 0;

            BarTaskPct.Value = 62;

            BarDiskC.Value   = 0;
            TxtDiskC.Text    = "—";
            BarDiskD.Value   = 0;
            TxtDiskD.Text    = "—";
            BarDiskNas.Value = 0;
            TxtDiskNas.Text  = "—";

            TxtOpHealth.Text = "NOMINAL";
            TxtOpAlerts.Text = "0";
            TxtOpTasks.Text = "2 active";
            OpWorkflowCard.CurrentStep = 0;
        }

        private void InitOperationLayer()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _operationTimer = q.CreateTimer();
            _operationTimer.Interval = TimeSpan.FromSeconds(2);
            _operationTimer.Tick += (_, _) => UpdateOperationLayer();
            _operationTimer.Start();
            UpdateOperationLayer();
        }

        private void UpdateOperationLayer()
        {
            _workflowStep = (_workflowStep + 1) % 7;
            OpWorkflowCard.CurrentStep = _workflowStep;

            TxtOpHealth.Text = _workflowStep >= 4 ? "ELEVATED" : "NOMINAL";
            TxtOpAlerts.Text = $"Alerts: {(_workflowStep >= 4 ? 2 : 0)}";
            TxtOpTasks.Text = $"Running Tasks: {2 + _workflowStep}";
        }

        private void InitNetWave()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _waveTimer = q.CreateTimer();
            _waveTimer.Interval = TimeSpan.FromMilliseconds(50);
            _waveTimer.Tick += (_, _) =>
            {
                _wavePhase += 0.12;
                DrawWave();
            };
            _waveTimer.Start();
        }

        private void DrawWave()
        {
            NetWaveCanvas.Children.Clear();
            var w = NetWaveCanvas.ActualWidth;
            var h = NetWaveCanvas.ActualHeight;
            if (w <= 0 || h <= 0) return;

            const int steps = 60;

            // グロー外層（先に追加して背面へ）
            var glowPts = new PointCollection();
            for (int i = 0; i <= steps; i++)
            {
                var x = i / (double)steps * w;
                var y = h / 2
                      - Math.Sin(i * Math.PI * 4 / steps + _wavePhase) * (h * 0.35)
                      - Math.Sin(i * Math.PI * 6 / steps + _wavePhase * 1.5) * (h * 0.12);
                glowPts.Add(new Windows.Foundation.Point(x, y));
            }
            var glow = new Polyline
            {
                Points          = glowPts,
                Stroke          = new SolidColorBrush(Color.FromArgb(30, 0, 247, 255)),
                StrokeThickness = 6,
                StrokeLineJoin  = PenLineJoin.Round
            };
            NetWaveCanvas.Children.Add(glow);

            // 本線（別 PointCollection）
            var linePts = new PointCollection();
            foreach (var p in glowPts)
                linePts.Add(p);

            var line = new Polyline
            {
                Points          = linePts,
                Stroke          = new SolidColorBrush(Color.FromArgb(180, 0, 247, 255)),
                StrokeThickness = 1.5,
                StrokeLineJoin  = PenLineJoin.Round
            };
            NetWaveCanvas.Children.Add(line);
        }

        private void OnWinMinClick(object sender, RoutedEventArgs e)
            => MainWindow.CurrentWindow?.MinimizeWindow();

        private void OnWinMaxClick(object sender, RoutedEventArgs e)
            => MainWindow.CurrentWindow?.ToggleMaximizeWindow();

        private void OnWinCloseClick(object sender, RoutedEventArgs e)
            => MainWindow.CurrentWindow?.CloseWindow();

        // ── Sidebar navigation ──────────────────────────────────────
        private void OnNavDashboard(object sender, RoutedEventArgs e)
        {
            // Already on Dashboard — no-op
        }

        private void OnNavWeather(object sender, RoutedEventArgs e)
            => Frame.Navigate(typeof(WeatherPage));

        // ── CardWeather tap → WeatherPage ──────────────────────────
        private void OnCardWeatherTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
            => Frame.Navigate(typeof(WeatherPage));
    }
}
