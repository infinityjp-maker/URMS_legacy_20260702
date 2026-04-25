using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;
using URMS.WinUI.Services;

namespace URMS.WinUI.Pages
{
    public sealed partial class DashboardPage : Page
    {
        private readonly LanguageService _lang = LanguageService.Instance;
        private DispatcherQueueTimer? _waveTimer;
        private double _wavePhase;

        public DashboardPage()
        {
            this.InitializeComponent();

            _lang.LanguageChanged              += (_, _) => Apply();
            ThemeService.Instance.ThemeChanged += (_, _) => Apply();

            this.Loaded   += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Apply();
            InitNetWave();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _waveTimer?.Stop();
            _waveTimer = null;
        }

        private void Apply()
        {
            var L = _lang;
            TxtSessionId.Text    = "URX-" + Guid.NewGuid().ToString("N")[..5].ToUpper();
            TxtCardSchedule.Text = L.Get("CardSchedule");
            TxtCardWeather.Text  = L.Get("CardWeather");
            TxtCardTask.Text     = L.Get("CardTask");
            TxtCardLauncher.Text = L.Get("CardLauncher");

            WxAnim.Condition    = "partly_cloudy";
            TxtWeatherTemp.Text = "22°C";
            TxtWeatherDesc.Text = "Tokyo · Minato";

            GaugeCpu.Value = 72;
            GaugeRam.Value = 58;
            GaugeGpu.Value = 45;

            BarTaskPct.Value = 62;

            BarDiskC.Value   = 68;
            TxtDiskC.Text    = "68%";
            BarDiskD.Value   = 42;
            TxtDiskD.Text    = "42%";
            BarDiskNas.Value = 31;
            TxtDiskNas.Text  = "31%";
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
    }
}
