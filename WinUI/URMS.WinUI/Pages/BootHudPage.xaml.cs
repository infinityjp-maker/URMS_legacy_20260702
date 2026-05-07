using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace URMS.WinUI.Pages
{
    public sealed partial class BootHudPage : Page
    {
        private DispatcherQueueTimer? _statusPulseTimer;
        private DispatcherQueueTimer? _scaleTimer;
        private DispatcherQueueTimer? _glowPulseTimer;
        private double _scaleT = 0;
        private bool   _scaleUp = true;
        private double _glowPhase = 0;

        // ─── 完了コールバック ────────────────────────────────────────────────
        /// <summary>フェードアウト完了後に親パネルへ通知するアクション</summary>
        public Action? TransitionComplete;

        public BootHudPage()
        {
            this.InitializeComponent();
            Opacity = 1;
            BootContentPanel.Opacity = 0;
            this.Loaded += OnBootHudLoaded;
        }

        // ═══════════════════════════════════════════════════
        // Loaded ハンドラ
        // ═══════════════════════════════════════════════════
        private async void OnBootHudLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnBootHudLoaded;

            // ContentScale の CenterX/CenterY を実サイズに設定
            BootContentPanel.Loaded += (_, _) =>
            {
                ContentScale.CenterX = BootContentPanel.ActualWidth / 2;
                ContentScale.CenterY = BootContentPanel.ActualHeight / 2;
            };

            // ① BootContentPanel フェードイン
            await FadeAsync(BootContentPanel, 0, 1, 220);

            // ② ロゴ フェードイン (600ms)
            await FadeAsync(UrmsLogo, 0, 1, 600);

            // ③ スケールパルス開始
            StartScalePulse();
            StartLogoGlowPulse();

            // ④ ステータスパルス開始
            StartStatusPulse();

            // ⑤ Boot シーケンス完走 (~2s)
            await Task.Delay(2000);

            // ⑥ "SYSTEM ONLINE" をフェードイン
            _statusPulseTimer?.Stop();
            BootStatusText.Opacity = 0;
            BootStatusText.Text = "SYSTEM ONLINE";
            await FadeAsync(BootStatusText, 0, 1, 500);
            await Task.Delay(250);

            // ⑦ BootContentPanel フェードアウト → 遷移完了
            await AnimateOpacityToDashboardAsync();
        }

        // ═══════════════════════════════════════════════════
        // スケールパルス (1.0 → 1.05 → 1.0)
        // ═══════════════════════════════════════════════════
        private void StartScalePulse()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _scaleTimer = q.CreateTimer();
            _scaleTimer.Interval = TimeSpan.FromMilliseconds(20);
            _scaleTimer.Tick += (_, _) =>
            {
                // 0.4s 周期 (20ms x 20 tick)
                double step = 0.10;
                if (_scaleUp)
                {
                    _scaleT += step;
                    if (_scaleT >= 1.0) { _scaleT = 1.0; _scaleUp = false; }
                }
                else
                {
                    _scaleT -= step;
                    if (_scaleT <= 0.0) { _scaleT = 0.0; _scaleUp = true; }
                }
                double s = 1.0 + 0.05 * _scaleT;
                ContentScale.ScaleX = s;
                ContentScale.ScaleY = s;
            };
            _scaleTimer.Start();
        }

        private void StartLogoGlowPulse()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _glowPulseTimer = q.CreateTimer();
            _glowPulseTimer.Interval = TimeSpan.FromMilliseconds(40);
            _glowPulseTimer.Tick += (_, _) =>
            {
                // 1.2s で 0.3 -> 0.0 を往復ではなく減衰ループ
                _glowPhase += (2 * Math.PI) / 30.0;
                double pulse = (Math.Sin(_glowPhase) + 1.0) * 0.5;
                LogoGlowPulse.Opacity = 0.3 * pulse;
            };
            _glowPulseTimer.Start();
        }

        // ═══════════════════════════════════════════════════
        // ステータスパルス
        // ═══════════════════════════════════════════════════
        private void StartStatusPulse()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _statusPulseTimer = q.CreateTimer();
            _statusPulseTimer.Interval = TimeSpan.FromMilliseconds(300);
            _statusPulseTimer.Tick += (_, _) =>
            {
                BootStatusText.Text = "INITIALIZING";
                BootStatusText.Opacity = BootStatusText.Opacity > 0.6 ? 0.35 : 1.0;
            };
            _statusPulseTimer.Start();
        }

        // ═══════════════════════════════════════════════════
        // BootContentPanel フェードアウト + 遷移完了
        // ═══════════════════════════════════════════════════
        private async Task AnimateOpacityToDashboardAsync()
        {
            _statusPulseTimer?.Stop();
            _scaleTimer?.Stop();
            _glowPulseTimer?.Stop();

            await FadeAsync(this, 1, 0, 400);

            // 起動完了時刻を記録
            App.RecordBootTransitionComplete();

            // 親グリッドから自身を削除（BootOverlayGrid.Children.Remove）
            TransitionComplete?.Invoke();
        }

        // ═══════════════════════════════════════════════════
        // 汎用 Opacity アニメーション (Storyboard)
        // ═══════════════════════════════════════════════════
        private static Task FadeAsync(UIElement target, double from, double to, int durationMs)
        {
            var tcs = new TaskCompletionSource<bool>();
            var anim = new DoubleAnimation
            {
                From     = from,
                To       = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(durationMs)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, "Opacity");
            var sb = new Storyboard();
            sb.Children.Add(anim);
            sb.Completed += (_, _) =>
            {
                target.Opacity = to;
                tcs.TrySetResult(true);
            };
            sb.Begin();
            return tcs.Task;
        }
    }
}
