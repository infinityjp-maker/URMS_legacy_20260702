using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Dispatching;
using System;
using System.Numerics;
using Windows.Foundation;

namespace URMS.WinUI.Controls
{
    [ContentProperty(Name = "CardContent")]
    public sealed partial class CyberCard : UserControl
    {
        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(CyberCard),
                new PropertyMetadata(null, (d, e) => ((CyberCard)d).InnerContent.Content = e.NewValue));

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        private DispatcherQueueTimer? _sweepTimer;
        private DispatcherQueueTimer? _hoverTimer;
        private double _sweepX = -1.2;
        private bool _sweeping = false;
        private double _hoverProgress = 0;

        private readonly TranslateTransform _hoverTranslate = new();
        private readonly ScaleTransform _hoverScale = new() { ScaleX = 1, ScaleY = 1 };

        public CyberCard()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var tg = new TransformGroup();
            tg.Children.Add(_hoverTranslate);
            tg.Children.Add(_hoverScale);
            this.RenderTransform = tg;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = e.NewSize.Width;
            double h = e.NewSize.Height;
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
                () => UpdateClip(w, h));
        }

        private static PathGeometry CreateCardGeometry(double w, double h)
        {
            const double cut = 14;
            var fig = new PathFigure { StartPoint = new Point(0, 0), IsClosed = true };
            fig.Segments.Add(new LineSegment { Point = new Point(w - cut, 0) });
            fig.Segments.Add(new LineSegment { Point = new Point(w, cut) });
            fig.Segments.Add(new LineSegment { Point = new Point(w, h) });
            fig.Segments.Add(new LineSegment { Point = new Point(cut, h) });
            fig.Segments.Add(new LineSegment { Point = new Point(0, h - cut) });

            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            return geo;
        }

        private void UpdateClip(double w, double h)
        {
            if (w <= 0 || h <= 0) return;

            var geo = CreateCardGeometry(w, h);
            // レイアウトサイクル防止: 先に Width/Height を固定してから Data を割当
            CardShape.Width = w;
            CardShape.Height = h;
            CardShape.Data = geo;

            SweepPath.Width = w;
            SweepPath.Height = h;
            SweepPath.Data = CreateCardGeometry(w, h);

        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Canvas.SetZIndex(this, 10);
            StartSweepAnimation();
            AnimateHover(true);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Canvas.SetZIndex(this, 0);
            SweepPath.Opacity = 0;
            _sweeping = false;
            _sweepTimer?.Stop();
            AnimateHover(false);
        }

        private void AnimateHover(bool entering)
        {
            _hoverTimer?.Stop();
            _hoverTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _hoverTimer.Interval = TimeSpan.FromMilliseconds(16);
            _hoverTimer.Tick += (_, _) =>
            {
                _hoverProgress += entering ? 0.14 : -0.14;
                _hoverProgress = Math.Clamp(_hoverProgress, 0, 1);
                _hoverTranslate.Y = 0;
                _hoverScale.ScaleX = 1 + 0.025 * _hoverProgress;
                _hoverScale.ScaleY = 1 + 0.025 * _hoverProgress;
                this.Translation = new Vector3(0, 0, (float)(20 * _hoverProgress));

                // サイバーグロー (#00E0FF, Opacity 0.25, フェーズ5)
                HoverGlow.Opacity = 0.25 * _hoverProgress;

                if (_hoverProgress <= 0 || _hoverProgress >= 1)
                    _hoverTimer!.Stop();
            };
            _hoverTimer.Start();
        }

        private void StartSweepAnimation()
        {
            if (_sweeping) return;
            _sweeping = true;
            _sweepX = -1.2;
            SweepBrushTransform.X = _sweepX;
            SweepPath.Opacity = 0.72;

            _sweepTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _sweepTimer.Interval = TimeSpan.FromMilliseconds(16);
            _sweepTimer.Tick += (_, _) =>
            {
                _sweepX += 0.04;
                SweepBrushTransform.X = _sweepX;
                if (_sweepX > 1.2)
                {
                    SweepPath.Opacity = 0;
                    _sweeping = false;
                    _sweepTimer!.Stop();
                }
            };
            _sweepTimer.Start();
        }
    }
}
