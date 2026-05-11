using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace URMS.WinUI.Controls
{
    /// <summary>
    /// 半円アナログメーター – Ellipse + StrokeDashArray 方式
    /// Value: 0-100, Label: 表示ラベル, GaugeColor: アクセントカラー
    /// </summary>
    public sealed partial class AnalogGauge : UserControl
    {
        // ── 依存関係プロパティ ────────────────────────────────────────────────
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(AnalogGauge),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(AnalogGauge),
                new PropertyMetadata(string.Empty, OnLabelChanged));

        public static readonly DependencyProperty GaugeColorProperty =
            DependencyProperty.Register(nameof(GaugeColor), typeof(Color), typeof(AnalogGauge),
                new PropertyMetadata(Color.FromArgb(255, 0, 212, 240), OnGaugeColorChanged));

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(AnalogGauge),
                new PropertyMetadata("%"));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, Math.Clamp(value, 0, 100));
        }
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        public Color GaugeColor
        {
            get => (Color)GetValue(GaugeColorProperty);
            set => SetValue(GaugeColorProperty, value);
        }
        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        // ── 定数 ─────────────────────────────────────────────────────────────
        private const double CX         = 50.0;
        private const double CY         = 50.0;
        private const double R          = 40.0;
        private const double NeedleLen  = 32.0;
        private const double StartDeg   = -210.0;
        private const double SweepDeg   = 240.0;
        private const double StrokeW    = 9.0;    // ArcValue/ArcBg の StrokeThickness
        private const double GlowW      = 13.0;   // ArcGlow の StrokeThickness

        // StrokeDashArray の「1 単位 = StrokeThickness 1px 分」に換算した満量ダッシュ
        private static readonly double DashFull = SweepDeg / 360.0 * (2 * Math.PI * R / StrokeW);
        private static readonly double GlowFull = SweepDeg / 360.0 * (2 * Math.PI * R / GlowW);

        // ── アニメーション用 ─────────────────────────────────────────────────
        private double _displayedValue = 0.0;

        public AnalogGauge()
        {
            this.InitializeComponent();
            this.Loaded += (_, _) => Draw(_displayedValue);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnalogGauge g) g.AnimateTo((double)e.NewValue);
        }
        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnalogGauge g) g.TxtLabel.Text = (string)e.NewValue;
        }
        private static void OnGaugeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnalogGauge g) g.Draw(g._displayedValue);
        }

        // ── 角度 → キャンバス座標 ────────────────────────────────────────────
        private static Point ArcPoint(double cx, double cy, double r, double angleDeg)
        {
            var rad = angleDeg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(rad), cy + r * Math.Sin(rad));
        }

        // ── 描画（Ellipse + StrokeDashArray） ─────────────────────────────────
        private void Draw(double val)
        {
            val = Math.Clamp(val, 0, 100);
            _displayedValue = val;

            double frac = val / 100.0;

            // 背景アーク（常に 240° 分を表示）
            ArcBg.StrokeDashArray = new DoubleCollection { DashFull, 9999.0 };

            // 値アーク
            double d = DashFull * frac;
            if (d > 0.02)
            {
                var brush = new SolidColorBrush(GaugeColor);
                ArcValue.Stroke          = brush;
                ArcGlow.Stroke           = brush;
                ArcValue.Visibility      = Visibility.Visible;
                ArcGlow.Visibility       = Visibility.Visible;
                ArcValue.StrokeDashArray = new DoubleCollection { d, 9999.0 };
                ArcGlow.StrokeDashArray  = new DoubleCollection { GlowFull * frac, 9999.0 };
            }
            else
            {
                ArcValue.Visibility = Visibility.Collapsed;
                ArcGlow.Visibility  = Visibility.Collapsed;
            }

            // 針
            double needleDeg = StartDeg + SweepDeg * frac;
            var np = ArcPoint(CX, CY, NeedleLen, needleDeg);
            var nb = ArcPoint(CX, CY, -8.0,      needleDeg);
            Needle.X1 = nb.X; Needle.Y1 = nb.Y;
            Needle.X2 = np.X; Needle.Y2 = np.Y;

            // テキスト
            TxtValue.Text       = Unit == "%" ? $"{val:0}%" : $"{val:0} {Unit}";
            TxtValue.Foreground = new SolidColorBrush(GaugeColor);
        }

        // ── EaseOut アニメーション ────────────────────────────────────────────
        private void AnimateTo(double targetVal)
        {
            var from  = _displayedValue;
            var delta = targetVal - from;
            const int steps = 30;
            int step = 0;

            var timer = Microsoft.UI.Dispatching.DispatcherQueue
                    .GetForCurrentThread()
                    ?.CreateTimer();

            if (timer == null) { Draw(targetVal); return; }

            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (_, _) =>
            {
                step++;
                double t     = (double)step / steps;
                double eased = 1 - Math.Pow(1 - t, 3);
                Draw(from + delta * eased);
                if (step >= steps)
                {
                    Draw(targetVal);
                    timer.Stop();
                }
            };
            timer.Start();
        }
    }
}


