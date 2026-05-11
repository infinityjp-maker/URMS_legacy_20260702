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

            // Phase 1: 0.8秒フェードイン
            await FadeAsync(BootContentPanel, 0, 1, 800);
            await FadeAsync(UrmsLogo, 0, 1, 800);

            // スケールパルス + ロゴグロー開始
            StartScalePulse();
            StartLogoGlowPulse();

            // Phase 2: 0.9秒点滅 (INITIALIZING)
            await BootInitializingBlink();

            // Phase 3: 0.32秒フェードアウト → 遷移完了
            await FadeAsync(this, 1, 0, 320);

            // 起動完了時刻を記録
            App.RecordBootTransitionComplete();

            // 親グリッドから自身を削除
            TransitionComplete?.Invoke();
        }

        // ═══════════════════════════════════════════════════
        // INITIALIZING 1.2秒点滅
        // ═══════════════════════════════════════════════════
        private Task BootInitializingBlink()
        {
            var tcs = new TaskCompletionSource<bool>();
            var q = DispatcherQueue.GetForCurrentThread();
            var blinkTimer = q.CreateTimer();
            blinkTimer.Interval = TimeSpan.FromMilliseconds(150);
            int tickCount = 0;
            int maxTicks = 6; // 900ms / 150ms = 6 ticks

            blinkTimer.Tick += (_, _) =>
            {
                tickCount++;
                // 0.15s周期で Opacity 1.0 ↔ 0.3
                BootStatusText.Opacity = (tickCount % 2 == 1) ? 1.0 : 0.3;
                if (tickCount >= maxTicks)
                {
                    blinkTimer.Stop();
                    BootStatusText.Opacity = 1.0;
                    tcs.TrySetResult(true);
                }
            };
            blinkTimer.Start();
            return tcs.Task;
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

        private void StopAllAnimations()
        {
            _scaleTimer?.Stop();
            _glowPulseTimer?.Stop();
        }

        private void StartLogoGlowPulse()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _glowPulseTimer = q.CreateTimer();
            _glowPulseTimer.Interval = TimeSpan.FromMilliseconds(30);
            _glowPulseTimer.Tick += (_, _) =>
            {
                // 0.6s で 0.3 -> 0.0 を往復ではなく減衰ループ
                _glowPhase += (2 * Math.PI) / 20.0;
                double pulse = (Math.Sin(_glowPhase) + 1.0) * 0.5;
                LogoGlowPulse.Opacity = 0.3 * pulse;
            };
            _glowPulseTimer.Start();
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
