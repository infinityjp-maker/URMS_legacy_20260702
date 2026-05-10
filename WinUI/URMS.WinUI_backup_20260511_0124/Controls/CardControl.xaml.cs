using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Numerics;
using Windows.UI;

namespace URMS.WinUI.Controls
{
    [ContentProperty(Name = "CardContent")]
    public sealed partial class CardControl : UserControl
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(100);

        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(CardControl),
                new PropertyMetadata(null, (d, e) => ((CardControl)d).InnerContent.Content = e.NewValue));

        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register(nameof(TitleText), typeof(string), typeof(CardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty MainValueTextProperty =
            DependencyProperty.Register(nameof(MainValueText), typeof(string), typeof(CardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubInfoTextProperty =
            DependencyProperty.Register(nameof(SubInfoText), typeof(string), typeof(CardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(nameof(IconGlyph), typeof(string), typeof(CardControl),
                new PropertyMetadata("\uE946", OnIconGlyphChanged));

        public static readonly DependencyProperty BaseGlowOpacityProperty =
            DependencyProperty.Register(nameof(BaseGlowOpacity), typeof(double), typeof(CardControl),
                new PropertyMetadata(0.18));

        public static readonly DependencyProperty HoverGlowOpacityProperty =
            DependencyProperty.Register(nameof(HoverGlowOpacity), typeof(double), typeof(CardControl),
                new PropertyMetadata(0.19));

        public static readonly DependencyProperty IsPrimaryToneProperty =
            DependencyProperty.Register(nameof(IsPrimaryTone), typeof(bool), typeof(CardControl),
                new PropertyMetadata(false));

        private Microsoft.UI.Composition.SpriteVisual? _shadowVisual;
        private Microsoft.UI.Composition.DropShadow? _dropShadow;

        public object CardContent
        {
            get => GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public string TitleText
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

        public string MainValueText
        {
            get => (string)GetValue(MainValueTextProperty);
            set => SetValue(MainValueTextProperty, value);
        }

        public string SubInfoText
        {
            get => (string)GetValue(SubInfoTextProperty);
            set => SetValue(SubInfoTextProperty, value);
        }

        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public double BaseGlowOpacity
        {
            get => (double)GetValue(BaseGlowOpacityProperty);
            set => SetValue(BaseGlowOpacityProperty, value);
        }

        public double HoverGlowOpacity
        {
            get => (double)GetValue(HoverGlowOpacityProperty);
            set => SetValue(HoverGlowOpacityProperty, value);
        }

        public bool IsPrimaryTone
        {
            get => (bool)GetValue(IsPrimaryToneProperty);
            set => SetValue(IsPrimaryToneProperty, value);
        }

        public CardControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        private static void OnIconGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardControl card && e.NewValue is string glyph)
            {
                card.InfoIcon.Glyph = glyph;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupCompositionShadow();
            ApplyMaterialProfile();
            InfoIcon.Glyph = IconGlyph;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_shadowVisual != null)
            {
                _shadowVisual.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
            }
        }

        private void SetupCompositionShadow()
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(ShadowHost);
            var compositor = hostVisual.Compositor;

            var dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = IsPrimaryTone ? 18f : 16f;
            dropShadow.Opacity = 0.36f;
            dropShadow.Color = Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0C, 0x1A, 0x2A);
            dropShadow.Offset = new Vector3(0f, IsPrimaryTone ? 11f : 9f, 0f);
            _dropShadow = dropShadow;

            _shadowVisual = compositor.CreateSpriteVisual();
            _shadowVisual.Shadow = dropShadow;
            _shadowVisual.Size = new Vector2((float)ActualWidth, (float)ActualHeight);

            ElementCompositionPreview.SetElementChildVisual(ShadowHost, _shadowVisual);
        }

        private void ApplyMaterialProfile()
        {
            if (IsPrimaryTone)
            {
                NoiseLayer.Opacity = 0.012;
                SatinReflection.Opacity = 0.036;
                SoftInnerReflection.Opacity = 0.075;
                TopHighlight.Opacity = 0.34;

                if (_dropShadow != null)
                {
                    _dropShadow.BlurRadius = 18f;
                    _dropShadow.Offset = new Vector3(0f, 11f, 0f);
                }
            }
            else
            {
                NoiseLayer.Opacity = 0.010;
                SatinReflection.Opacity = 0.030;
                SoftInnerReflection.Opacity = 0.060;
                TopHighlight.Opacity = 0.28;

                if (_dropShadow != null)
                {
                    _dropShadow.BlurRadius = 16f;
                    _dropShadow.Offset = new Vector3(0f, 9f, 0f);
                }
            }
        }

        private void ApplyState(bool isHover, bool isActive)
        {
            var targetGlow = isHover ? HoverGlowOpacity : BaseGlowOpacity;
            var targetOverlay = isActive ? 0.045 : 0.00;
            var targetScale = isHover ? 1.004 : 1.000;
            var targetY = isHover ? -0.5 : 0.0;
            var targetInner = IsPrimaryTone ? (isHover ? 0.082 : 0.075) : (isHover ? 0.066 : 0.060);
            var targetSatin = IsPrimaryTone ? (isHover ? 0.040 : 0.036) : (isHover ? 0.033 : 0.030);

            AnimateDouble(GlowBorder, "Opacity", targetGlow);
            AnimateDouble(ActiveOverlay, "Opacity", targetOverlay);
            AnimateDouble(SoftInnerReflection, "Opacity", targetInner);
            AnimateDouble(SatinReflection, "Opacity", targetSatin);
            AnimateDouble(CardRoot, "(UIElement.Scale).X", targetScale);
            AnimateDouble(CardRoot, "(UIElement.Scale).Y", targetScale);
            AnimateDouble(CardRoot, "(UIElement.Translation).Y", targetY);

            if (isActive)
            {
                var activeTop = IsPrimaryTone ? BrightenByPercent(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0B, 0x10, 0x1A), 0.06) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0B, 0x10, 0x1A);
                var activeBottom = IsPrimaryTone ? BrightenByPercent(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0E, 0x13, 0x1E), 0.06) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0E, 0x13, 0x1E);
                BgStopTop.Color = activeTop;
                BgStopBottom.Color = activeBottom;
            }
            else
            {
                var baseTop = IsPrimaryTone ? BrightenByPercent(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0A, 0x0F, 0x1A), 0.06) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0A, 0x0F, 0x1A);
                var baseBottom = IsPrimaryTone ? BrightenByPercent(Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0D, 0x12, 0x1E), 0.06) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0D, 0x12, 0x1E);
                BgStopTop.Color = baseTop;
                BgStopBottom.Color = baseBottom;
            }
        }

        private static Color BrightenByPercent(Color color, double percent)
        {
            var factor = 1.0 + percent;
            byte r = (byte)Math.Min(255, (int)Math.Round(color.R * factor));
            byte g = (byte)Math.Min(255, (int)Math.Round(color.G * factor));
            byte b = (byte)Math.Min(255, (int)Math.Round(color.B * factor));
            return Microsoft.UI.ColorHelper.FromArgb(color.A, r, g, b);
        }

        private void AnimateDouble(DependencyObject target, string path, double to)
        {
            var animation = new DoubleAnimation
            {
                To = to,
                Duration = new Duration(TransitionDuration),
                EnableDependentAnimation = true
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, path);
            storyboard.Begin();
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ApplyState(isHover: true, isActive: false);
        }

        private void OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            ApplyState(isHover: false, isActive: false);
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ApplyState(isHover: true, isActive: true);
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ApplyState(isHover: true, isActive: false);
        }
    }
}
