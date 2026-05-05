using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;

namespace URMS.WinUI.Pages
{
    public sealed partial class BootHudPage : Page
    {
        // ─── ステータス文字列（パルス） ──────────────────────────────────────
        private static readonly string[] _statusSeq =
        [
            "INITIALIZING", "LOADING MODULES", "SYNC NODES",
            "CALIBRATING", "ACTIVATING HUD", "SYSTEM READY"
        ];
        private int _statusIdx = 0;

        private DispatcherQueueTimer? _statusPulseTimer;
        private DispatcherQueueTimer? _scaleTimer;
        private double _scaleT = 0;
        private bool   _scaleUp = true;

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

            // ② ロゴ フェードイン (300ms)
            await FadeAsync(UrmsLogo, 0, 1, 300);

            // ③ スケールパルス開始
            StartScalePulse();

            // ④ ステータスパルス開始
            StartStatusPulse();

            // ⑤ Boot シーケンス完走 (~2s)
            await Task.Delay(2000);

            // ⑥ 全ステータスを "SYSTEM READY" に
            BootStatusText.Text = "SYSTEM READY";
            await Task.Delay(400);

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
                double step = 0.004;
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

        // ═══════════════════════════════════════════════════
        // ステータスパルス
        // ═══════════════════════════════════════════════════
        private void StartStatusPulse()
        {
            var q = DispatcherQueue.GetForCurrentThread();
            _statusPulseTimer = q.CreateTimer();
            _statusPulseTimer.Interval = TimeSpan.FromMilliseconds(400);
            _statusPulseTimer.Tick += (_, _) =>
            {
                _statusIdx = (_statusIdx + 1) % _statusSeq.Length;
                BootStatusText.Text = _statusSeq[_statusIdx];
                BootStatusText.Opacity = BootStatusText.Opacity > 0.5 ? 0.55 : 1.0;
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

            await FadeAsync(BootContentPanel, 1, 0, 500);

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
