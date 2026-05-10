using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Numerics;
using Windows.UI;

namespace URMS.WinUI.Controls
{
    [ContentProperty(Name = "CardContent")]
    public sealed partial class CardControl : UserControl
    {
        private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(140);

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
                new PropertyMetadata(false, OnProfileChanged));

        public static readonly DependencyProperty MaterialIntensityProperty =
            DependencyProperty.Register(nameof(MaterialIntensity), typeof(double), typeof(CardControl),
                new PropertyMetadata(1.0, OnProfileChanged));

        public static readonly DependencyProperty OpticalDepthProperty =
            DependencyProperty.Register(nameof(OpticalDepth), typeof(double), typeof(CardControl),
                new PropertyMetadata(1.0, OnProfileChanged));

        public static readonly DependencyProperty ShadowDepthProperty =
            DependencyProperty.Register(nameof(ShadowDepth), typeof(double), typeof(CardControl),
                new PropertyMetadata(1.0, OnProfileChanged));

        public static readonly DependencyProperty InfoDensityProperty =
            DependencyProperty.Register(nameof(InfoDensity), typeof(double), typeof(CardControl),
                new PropertyMetadata(1.0, OnProfileChanged));

        private Compositor? _compositor;
        private Visual? _cardRootVisual;
        private Visual? _glowVisual;
        private Visual? _activeOverlayVisual;
        private Visual? _satinVisual;
        private Visual? _innerReflectionVisual;
        private Visual? _reflectionWaveVisual;
        private Visual? _backdropVisual;
        private ShadowLayer? _deepShadowLayer;
        private ShadowLayer? _midShadowLayer;
        private ShadowLayer? _liftShadowLayer;

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

        public double MaterialIntensity
        {
            get => (double)GetValue(MaterialIntensityProperty);
            set => SetValue(MaterialIntensityProperty, value);
        }

        public double OpticalDepth
        {
            get => (double)GetValue(OpticalDepthProperty);
            set => SetValue(OpticalDepthProperty, value);
        }

        public double ShadowDepth
        {
            get => (double)GetValue(ShadowDepthProperty);
            set => SetValue(ShadowDepthProperty, value);
        }

        public double InfoDensity
        {
            get => (double)GetValue(InfoDensityProperty);
            set => SetValue(InfoDensityProperty, value);
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

        private static void OnProfileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CardControl card && card.IsLoaded)
            {
                card.ApplyMaterialProfile();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetupComposition();
            ApplyMaterialProfile();
            InfoIcon.Glyph = IconGlyph;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateVisualMetrics((float)e.NewSize.Width, (float)e.NewSize.Height);
        }

        private void SetupComposition()
        {
            if (_compositor != null)
            {
                UpdateVisualMetrics((float)ActualWidth, (float)ActualHeight);
                return;
            }

            var hostVisual = ElementCompositionPreview.GetElementVisual(CardRoot);
            _compositor = hostVisual.Compositor;
            _cardRootVisual = hostVisual;
            _glowVisual = ElementCompositionPreview.GetElementVisual(GlowBorder);
            _activeOverlayVisual = ElementCompositionPreview.GetElementVisual(ActiveOverlay);
            _satinVisual = ElementCompositionPreview.GetElementVisual(SatinReflection);
            _innerReflectionVisual = ElementCompositionPreview.GetElementVisual(SoftInnerReflection);
            _reflectionWaveVisual = ElementCompositionPreview.GetElementVisual(ReflectionWave);
            _backdropVisual = ElementCompositionPreview.GetElementVisual(CardBackdropBlur);

            _deepShadowLayer = CreateShadowLayer(DeepShadowHost);
            _midShadowLayer = CreateShadowLayer(MidShadowHost);
            _liftShadowLayer = CreateShadowLayer(LiftShadowHost);
            UpdateVisualMetrics((float)ActualWidth, (float)ActualHeight);
        }

        private void ApplyMaterialProfile()
        {
            var profile = BuildProfile();

            NoiseLayer.Opacity = profile.NoiseOpacity;
            SatinReflection.Opacity = profile.SatinOpacity;
            InnerGlow.Opacity = profile.InnerGlowOpacity;
            TopHighlight.Opacity = profile.TopHighlightOpacity;
            SoftInnerReflection.Opacity = profile.SoftInnerReflectionOpacity;
            GlowBorder.Opacity = BaseGlowOpacity;
            CardBackdropBlur.Opacity = profile.BlurOpacity;
            ActiveOverlay.Opacity = 0.0;
            ReflectionWave.Opacity = 0.0;

            InfoPanel.Margin = new Thickness(52, 44, 52, 44);
            MainValueTextBlock.FontSize = profile.MainValueFontSize;
            MainValueTextBlock.CharacterSpacing = IsPrimaryTone ? 30 : 24;
            TitleTextBlock.FontSize = profile.TitleFontSize;
            SubInfoTextBlock.FontSize = profile.SubInfoFontSize;
            SubInfoTextBlock.LineHeight = profile.SubInfoLineHeight;

            BgStopTop.Color = profile.BackgroundTop;
            BgStopMid.Color = profile.BackgroundMid;
            BgStopBottom.Color = profile.BackgroundBottom;
            CardBackground.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(profile.BorderColor);
            MaterialFrame.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(profile.FrameColor);
            DeepOuterShadow.BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(profile.DeepOuterShadowColor);

            ApplyShadowProfile(_deepShadowLayer, profile.DeepShadow);
            ApplyShadowProfile(_midShadowLayer, profile.MidShadow);
            ApplyShadowProfile(_liftShadowLayer, profile.LiftShadow);
            UpdateVisualMetrics((float)ActualWidth, (float)ActualHeight);
        }

        private void ApplyState(bool isHover, bool isActive)
        {
            var profile = BuildProfile();
            var emphasis = IsPrimaryTone ? 1.15f : 1.0f;
            var targetScale = isHover ? 1.008f * emphasis : 1.0f;
            var targetY = isHover ? -2.4f * emphasis : 0.0f;
            var targetGlow = (float)(isHover ? HoverGlowOpacity : BaseGlowOpacity);
            var targetOverlay = (float)(isActive ? profile.ActiveOverlayOpacity : 0.0);
            var targetSatin = (float)(profile.SatinOpacity + (isHover ? 0.03 : 0.0));
            var targetInner = (float)(profile.SoftInnerReflectionOpacity + (isHover ? 0.03 : 0.0));
            var targetWave = (float)(isHover ? 0.09 : 0.0);
            var targetBlur = (float)(profile.BlurOpacity + (isHover ? 0.04 : 0.0));

            AnimateOpacity(_glowVisual, targetGlow);
            AnimateOpacity(_activeOverlayVisual, targetOverlay);
            AnimateOpacity(_satinVisual, targetSatin);
            AnimateOpacity(_innerReflectionVisual, targetInner);
            AnimateOpacity(_reflectionWaveVisual, targetWave);
            AnimateOpacity(_backdropVisual, targetBlur);
            AnimateCardTransform(targetScale, targetY);

            if (_liftShadowLayer?.Shadow != null)
            {
                AnimateScalar(_liftShadowLayer.Shadow, nameof(DropShadow.Opacity), isHover ? 0.30f : profile.LiftShadow.Opacity);
                AnimateVector3(_liftShadowLayer.Shadow, nameof(DropShadow.Offset), new Vector3(0f, isHover ? profile.LiftShadow.OffsetY - 1.2f : profile.LiftShadow.OffsetY, 0f));
            }

            if (_midShadowLayer?.Shadow != null)
            {
                AnimateScalar(_midShadowLayer.Shadow, nameof(DropShadow.BlurRadius), isHover ? profile.MidShadow.BlurRadius + 1.8f : profile.MidShadow.BlurRadius);
            }

            if (isActive)
            {
                BgStopTop.Color = BrightenByPercent(profile.BackgroundTop, 0.05);
                BgStopMid.Color = BrightenByPercent(profile.BackgroundMid, 0.05);
                BgStopBottom.Color = BrightenByPercent(profile.BackgroundBottom, 0.05);
            }
            else
            {
                BgStopTop.Color = profile.BackgroundTop;
                BgStopMid.Color = profile.BackgroundMid;
                BgStopBottom.Color = profile.BackgroundBottom;
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

        private ShadowLayer CreateShadowLayer(FrameworkElement host)
        {
            var compositor = _compositor!;
            var sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Microsoft.UI.ColorHelper.FromArgb(1, 255, 255, 255));
            var shadow = compositor.CreateDropShadow();
            sprite.Shadow = shadow;
            ElementCompositionPreview.SetElementChildVisual(host, sprite);
            return new ShadowLayer(host, sprite, shadow);
        }

        private void UpdateVisualMetrics(float width, float height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var size = new Vector2(width, height);
            SetShadowSize(_deepShadowLayer, size);
            SetShadowSize(_midShadowLayer, size);
            SetShadowSize(_liftShadowLayer, size);

            if (_cardRootVisual != null)
            {
                _cardRootVisual.CenterPoint = new Vector3(width / 2f, height / 2f, 0f);
            }
        }

        private static void SetShadowSize(ShadowLayer? layer, Vector2 size)
        {
            if (layer?.Visual != null)
            {
                layer.Visual.Size = size;
            }
        }

        private void ApplyShadowProfile(ShadowLayer? layer, ShadowSpec spec)
        {
            if (layer?.Shadow == null)
            {
                return;
            }

            layer.Shadow.BlurRadius = spec.BlurRadius;
            layer.Shadow.Opacity = spec.Opacity;
            layer.Shadow.Color = spec.Color;
            layer.Shadow.Offset = new Vector3(0f, spec.OffsetY, 0f);
        }

        private CardProfile BuildProfile()
        {
            var primaryBoost = IsPrimaryTone ? 1.2 : 1.0;
            var material = Math.Clamp(MaterialIntensity * primaryBoost, 0.75, 1.8);
            var optical = Math.Clamp(OpticalDepth * (IsPrimaryTone ? 1.3 : 1.0), 0.65, 1.9);
            var shadow = Math.Clamp(ShadowDepth * (IsPrimaryTone ? 1.4 : 1.0), 0.7, 2.2);
            var density = Math.Clamp(InfoDensity, 0.8, 1.2);

            var baseTop = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x14, 0x1E, 0x2E) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x10, 0x18, 0x26);
            var baseMid = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0D, 0x16, 0x24) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0C, 0x13, 0x20);
            var baseBottom = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x09, 0x11, 0x1B) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x08, 0x10, 0x1A);

            return new CardProfile(
                NoiseOpacity: 0.012 * material,
                SatinOpacity: 0.035 * material,
                InnerGlowOpacity: 0.080 * material,
                BlurOpacity: 0.30 + ((material - 1.0) * 0.08),
                TopHighlightOpacity: 0.28 * optical,
                SoftInnerReflectionOpacity: 0.085 * optical,
                ActiveOverlayOpacity: IsPrimaryTone ? 0.076 : 0.058,
                MainValueFontSize: IsPrimaryTone ? 36 + ((density - 1.0) * 4) : 32 + ((density - 1.0) * 3),
                TitleFontSize: 12,
                SubInfoFontSize: 13,
                SubInfoLineHeight: 24,
                BackgroundTop: baseTop,
                BackgroundMid: baseMid,
                BackgroundBottom: baseBottom,
                BorderColor: Microsoft.UI.ColorHelper.FromArgb(0x46, 0x78, 0x93, 0xB3),
                FrameColor: Microsoft.UI.ColorHelper.FromArgb(0x20, 0x16, 0x20, 0x2D),
                DeepOuterShadowColor: Microsoft.UI.ColorHelper.FromArgb(0x2D, 0x07, 0x0A, 0x0F),
                DeepShadow: new ShadowSpec(
                    BlurRadius: 18f + (float)((shadow - 1.0) * 6.0),
                    Opacity: 0.30f,
                    OffsetY: 12f + (float)((shadow - 1.0) * 4.0),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x05, 0x08, 0x10)),
                MidShadow: new ShadowSpec(
                    BlurRadius: 12f + (float)((shadow - 1.0) * 4.0),
                    Opacity: 0.18f,
                    OffsetY: 7f + (float)((shadow - 1.0) * 2.5),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x08, 0x0D, 0x16)),
                LiftShadow: new ShadowSpec(
                    BlurRadius: 24f,
                    Opacity: 0.24f,
                    OffsetY: 15f + (float)((shadow - 1.0) * 5.0),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x03, 0x05, 0x0A)));
        }

        private void AnimateCardTransform(float scale, float offsetY)
        {
            if (_cardRootVisual == null)
            {
                return;
            }

            AnimateVector3(_cardRootVisual, nameof(Visual.Scale), new Vector3(scale, scale, 1f));
            AnimateVector3(_cardRootVisual, nameof(Visual.Offset), new Vector3(0f, offsetY, 0f));
        }

        private void AnimateOpacity(Visual? visual, float opacity)
        {
            if (visual == null)
            {
                return;
            }

            AnimateScalar(visual, nameof(Visual.Opacity), opacity);
        }

        private void AnimateScalar(CompositionObject target, string propertyName, float value)
        {
            if (_compositor == null)
            {
                return;
            }

            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = GetAnimationDuration();
            target.StartAnimation(propertyName, animation);
        }

        private void AnimateVector3(CompositionObject target, string propertyName, Vector3 value)
        {
            if (_compositor == null)
            {
                return;
            }

            var animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = GetAnimationDuration();
            target.StartAnimation(propertyName, animation);
        }

        private TimeSpan GetAnimationDuration()
            => IsPrimaryTone ? TimeSpan.FromMilliseconds(TransitionDuration.TotalMilliseconds * 1.15) : TransitionDuration;

        private sealed record ShadowLayer(FrameworkElement Host, SpriteVisual Visual, DropShadow Shadow);

        private sealed record ShadowSpec(float BlurRadius, float Opacity, float OffsetY, Color Color);

        private sealed record CardProfile(
            double NoiseOpacity,
            double SatinOpacity,
            double InnerGlowOpacity,
            double BlurOpacity,
            double TopHighlightOpacity,
            double SoftInnerReflectionOpacity,
            double ActiveOverlayOpacity,
            double MainValueFontSize,
            double TitleFontSize,
            double SubInfoFontSize,
            double SubInfoLineHeight,
            Color BackgroundTop,
            Color BackgroundMid,
            Color BackgroundBottom,
            Color BorderColor,
            Color FrameColor,
            Color DeepOuterShadowColor,
            ShadowSpec DeepShadow,
            ShadowSpec MidShadow,
            ShadowSpec LiftShadow);

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
