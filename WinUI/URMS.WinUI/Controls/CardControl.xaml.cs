using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;
using Windows.UI;

namespace URMS.WinUI.Controls
{
    [ContentProperty(Name = "CardContent")]
    public sealed partial class CardControl : UserControl
    {
        private static readonly TimeSpan HoverDuration = TimeSpan.FromMilliseconds(160);
        private static readonly TimeSpan ActiveDuration = TimeSpan.FromMilliseconds(180);

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
        private Visual? _ambientFogVisual;
        private Visual? _backdropBloomVisual;
        private Visual? _heroAuraVisual;
        private Visual? _heroBandVisual;
        private Visual? _tierRailVisual;
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
            _ambientFogVisual = ElementCompositionPreview.GetElementVisual(AmbientFog);
            _backdropBloomVisual = ElementCompositionPreview.GetElementVisual(BackdropBloom);
            _heroAuraVisual = ElementCompositionPreview.GetElementVisual(HeroAura);
            _heroBandVisual = ElementCompositionPreview.GetElementVisual(HeroBand);
            _tierRailVisual = ElementCompositionPreview.GetElementVisual(TierRail);

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
            HeroAura.Opacity = profile.HeroAuraOpacity;
            HeroBand.Opacity = profile.HeroBandOpacity;
            TierRail.Opacity = profile.TierRailOpacity;
            TierBadge.Opacity = profile.BadgeOpacity;

            InfoPanel.Margin = profile.ContentMargin;
            MainValueTextBlock.FontSize = profile.MainValueFontSize;
            MainValueTextBlock.CharacterSpacing = IsPrimaryTone ? 30 : 24;
            TitleTextBlock.FontSize = profile.TitleFontSize;
            SubInfoTextBlock.FontSize = profile.SubInfoFontSize;
            SubInfoTextBlock.LineHeight = profile.SubInfoLineHeight;
            TierBadgeText.Text = profile.TierLabel;

            BgStopTop.Color = profile.BackgroundTop;
            BgStopMid.Color = profile.BackgroundMid;
            BgStopBottom.Color = profile.BackgroundBottom;
            CardBackground.BorderBrush = new SolidColorBrush(profile.BorderColor);
            MaterialFrame.BorderBrush = new SolidColorBrush(profile.FrameColor);
            DeepOuterShadow.BorderBrush = new SolidColorBrush(profile.DeepOuterShadowColor);
            InfoIconHost.Background = new SolidColorBrush(profile.IconBackgroundColor);
            InfoIconHost.BorderBrush = new SolidColorBrush(profile.IconBorderColor);
            InfoIcon.Foreground = new SolidColorBrush(profile.IconForegroundColor);
            TitleTextBlock.Foreground = new SolidColorBrush(profile.TitleColor);
            MainValueTextBlock.Foreground = new SolidColorBrush(profile.ValueColor);
            SubInfoTextBlock.Foreground = new SolidColorBrush(profile.SubInfoColor);
            TierBadge.Background = new SolidColorBrush(profile.BadgeBackgroundColor);
            TierBadge.BorderBrush = new SolidColorBrush(profile.BadgeBorderColor);
            TierBadgeText.Foreground = new SolidColorBrush(profile.BadgeTextColor);
            TitleDivider.Opacity = profile.DividerOpacity;

            ApplyShadowProfile(_deepShadowLayer, profile.DeepShadow);
            ApplyShadowProfile(_midShadowLayer, profile.MidShadow);
            ApplyShadowProfile(_liftShadowLayer, profile.LiftShadow);
            UpdateVisualMetrics((float)ActualWidth, (float)ActualHeight);
        }

        private void ApplyState(bool isHover, bool isActive)
        {
            var profile = BuildProfile();
            var emphasis = IsPrimaryTone ? 1.30f : 1.0f;
            var targetScale = isHover ? 1.010f * emphasis : 1.0f;
            var targetY = isHover ? -2.8f * emphasis : 0.0f;
            var targetGlow = (float)(isHover ? HoverGlowOpacity : BaseGlowOpacity);
            var targetOverlay = (float)(isActive ? profile.ActiveOverlayOpacity : 0.0);
            var targetSatin = (float)(profile.SatinOpacity + (isHover ? 0.06 : 0.0) + (isActive ? 0.10 : 0.0));
            var targetInner = (float)(profile.SoftInnerReflectionOpacity + (isHover ? 0.05 : 0.0));
            var targetWave = (float)(isHover ? 0.09 : 0.0);
            var targetBlur = (float)(profile.BlurOpacity + (isHover ? 0.04 : 0.0));
            var targetFog = (float)(profile.AmbientFogOpacity + (isHover ? 0.01 : 0.0));
            var targetBloom = (float)(profile.BloomOpacity + (isHover ? 0.03 : 0.0) + (isActive ? 0.03 : 0.0));
            var targetHeroAura = (float)(profile.HeroAuraOpacity + (isHover ? 0.06 : 0.0) + (isActive ? 0.04 : 0.0));
            var targetHeroBand = (float)(profile.HeroBandOpacity + (isHover ? 0.05 : 0.0));
            var targetTierRail = (float)(profile.TierRailOpacity + (isHover ? 0.04 : 0.0));
            var duration = GetAnimationDuration(isActive);

            AnimateOpacity(_glowVisual, targetGlow, duration);
            AnimateOpacity(_activeOverlayVisual, targetOverlay, duration);
            AnimateOpacity(_satinVisual, targetSatin, duration);
            AnimateOpacity(_innerReflectionVisual, targetInner, duration);
            AnimateOpacity(_reflectionWaveVisual, targetWave, duration);
            AnimateOpacity(_backdropVisual, targetBlur, duration);
            AnimateOpacity(_ambientFogVisual, targetFog, duration);
            AnimateOpacity(_backdropBloomVisual, targetBloom, duration);
            AnimateOpacity(_heroAuraVisual, targetHeroAura, duration);
            AnimateOpacity(_heroBandVisual, targetHeroBand, duration);
            AnimateOpacity(_tierRailVisual, targetTierRail, duration);
            AnimateCardTransform(targetScale, targetY, duration);

            if (_liftShadowLayer?.Shadow != null)
            {
                AnimateScalar(_liftShadowLayer.Shadow, nameof(DropShadow.Opacity), isHover ? 0.34f : profile.LiftShadow.Opacity, duration);
                var hoverLift = isHover ? profile.LiftShadow.OffsetY + 6f : profile.LiftShadow.OffsetY;
                var activeLift = isActive ? hoverLift + 10f : hoverLift;
                AnimateVector3(_liftShadowLayer.Shadow, nameof(DropShadow.Offset), new Vector3(0f, activeLift, 0f), duration);
            }

            if (_midShadowLayer?.Shadow != null)
            {
                AnimateScalar(_midShadowLayer.Shadow, nameof(DropShadow.BlurRadius), isHover ? profile.MidShadow.BlurRadius + 6f : profile.MidShadow.BlurRadius, duration);
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
            var material = Math.Clamp(MaterialIntensity * primaryBoost, 0.8, 2.4);
            var optical = Math.Clamp(OpticalDepth * (IsPrimaryTone ? 1.4 : 1.0), 0.65, 2.2);
            var shadow = Math.Clamp(ShadowDepth * (IsPrimaryTone ? 1.4 : 1.0), 0.7, 2.2);
            var density = Math.Clamp(InfoDensity, 0.8, 1.2);
            var tierLabel = IsPrimaryTone ? "SYSTEM" : MaterialIntensity <= 0.8 ? "OPERATION" : "SUBSYSTEM";

            var baseTop = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x18, 0x26, 0x3A) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0C, 0x14, 0x20) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x12, 0x1B, 0x29);
            var baseMid = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x11, 0x1B, 0x2C) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x08, 0x10, 0x19) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0D, 0x15, 0x22);
            var baseBottom = IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x0A, 0x12, 0x1D) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x05, 0x0A, 0x11) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x08, 0x0F, 0x18);

            return new CardProfile(
                NoiseOpacity: Math.Clamp(0.020 + ((material - 1.0) * 0.010), 0.016, 0.038),
                SatinOpacity: Math.Clamp(0.08 + ((material - 1.0) * 0.05), 0.05, 0.16),
                InnerGlowOpacity: Math.Clamp(0.12 + ((material - 1.0) * 0.06), 0.08, 0.24),
                BlurOpacity: Math.Clamp(0.52 + ((material - 1.0) * 0.14), 0.40, 0.78),
                TopHighlightOpacity: Math.Clamp(0.20 * optical, 0.14, 0.52),
                SoftInnerReflectionOpacity: Math.Clamp(0.09 * optical, 0.05, 0.24),
                ActiveOverlayOpacity: IsPrimaryTone ? 0.17 : 0.10,
                AmbientFogOpacity: IsPrimaryTone ? 0.11 : MaterialIntensity <= 0.8 ? 0.03 : 0.06,
                BloomOpacity: IsPrimaryTone ? 0.18 : 0.03,
                HeroAuraOpacity: IsPrimaryTone ? 0.22 : 0.0,
                HeroBandOpacity: IsPrimaryTone ? 0.26 : MaterialIntensity <= 0.8 ? 0.04 : 0.10,
                TierRailOpacity: IsPrimaryTone ? 0.55 : MaterialIntensity <= 0.8 ? 0.12 : 0.24,
                BadgeOpacity: IsPrimaryTone ? 1.0 : 0.88,
                MainValueFontSize: IsPrimaryTone ? 42 + ((density - 1.0) * 3) : MaterialIntensity <= 0.8 ? 28 + ((density - 1.0) * 2) : 34 + ((density - 1.0) * 2),
                TitleFontSize: 14,
                SubInfoFontSize: 12,
                SubInfoLineHeight: 26,
                BackgroundTop: baseTop,
                BackgroundMid: baseMid,
                BackgroundBottom: baseBottom,
                BorderColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x88, 0xB2, 0xD6, 0xF2) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0x34, 0x6F, 0x88, 0xA1) : Microsoft.UI.ColorHelper.FromArgb(0x58, 0x92, 0xB6, 0xD4),
                FrameColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x58, 0xE0, 0xF0, 0xFF) : Microsoft.UI.ColorHelper.FromArgb(0x24, 0x33, 0x48, 0x5A),
                DeepOuterShadowColor: Microsoft.UI.ColorHelper.FromArgb(IsPrimaryTone ? (byte)0x48 : (byte)0x2D, 0x07, 0x0A, 0x0F),
                IconBackgroundColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x34, 0x46, 0x6B, 0x8A) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0x22, 0x2B, 0x40, 0x54) : Microsoft.UI.ColorHelper.FromArgb(0x24, 0x36, 0x52, 0x6A),
                IconBorderColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x98, 0xD7, 0xEC, 0xFF) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0x46, 0x8D, 0xB2, 0xCF) : Microsoft.UI.ColorHelper.FromArgb(0x68, 0xB4, 0xD6, 0xEE),
                IconForegroundColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xF2, 0xF9, 0xFF) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xCC, 0xE6, 0xFA),
                TitleColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xDB, 0xEC, 0xFA) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x98, 0xAD, 0xC0) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xB9, 0xD0, 0xE4),
                ValueColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xFA, 0xFD, 0xFF) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xD8, 0xE6, 0xF2) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xEE, 0xF5, 0xFC),
                SubInfoColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xB9, 0xCC, 0xDE) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x86, 0x9A, 0xAD) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x9E, 0xB3, 0xC8),
                BadgeBackgroundColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x42, 0x3B, 0x59, 0x76) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0x24, 0x2A, 0x40, 0x52) : Microsoft.UI.ColorHelper.FromArgb(0x30, 0x34, 0x4B, 0x62),
                BadgeBorderColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0x86, 0xC9, 0xE5, 0xFA) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0x44, 0x86, 0xA6, 0xC2) : Microsoft.UI.ColorHelper.FromArgb(0x5A, 0xAC, 0xCC, 0xE4),
                BadgeTextColor: IsPrimaryTone ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xF1, 0xF8, 0xFF) : MaterialIntensity <= 0.8 ? Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xB8, 0xD0, 0xE2) : Microsoft.UI.ColorHelper.FromArgb(0xFF, 0xD7, 0xE8, 0xF6),
                DividerOpacity: IsPrimaryTone ? 0.72 : MaterialIntensity <= 0.8 ? 0.20 : 0.42,
                TierLabel: tierLabel,
                ContentMargin: IsPrimaryTone ? new Thickness(60, 54, 60, 58) : MaterialIntensity <= 0.8 ? new Thickness(44, 36, 44, 40) : new Thickness(52, 44, 52, 48),
                DeepShadow: new ShadowSpec(
                    BlurRadius: 34f + (float)((shadow - 1.0) * 16.0),
                    Opacity: IsPrimaryTone ? 0.42f : 0.28f,
                    OffsetY: 20f + (float)((shadow - 1.0) * 8.0),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x05, 0x08, 0x10)),
                MidShadow: new ShadowSpec(
                    BlurRadius: 12f + (float)((shadow - 1.0) * 6.0),
                    Opacity: IsPrimaryTone ? 0.22f : 0.16f,
                    OffsetY: 7f + (float)((shadow - 1.0) * 2.5),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x08, 0x0D, 0x16)),
                LiftShadow: new ShadowSpec(
                    BlurRadius: IsPrimaryTone ? 30f : 22f,
                    Opacity: IsPrimaryTone ? 0.30f : 0.18f,
                    OffsetY: 15f + (float)((shadow - 1.0) * 5.0),
                    Color: Microsoft.UI.ColorHelper.FromArgb(0xFF, 0x03, 0x05, 0x0A)));
        }

        private void AnimateCardTransform(float scale, float offsetY, TimeSpan duration)
        {
            if (_cardRootVisual == null)
            {
                return;
            }

            AnimateVector3(_cardRootVisual, nameof(Visual.Scale), new Vector3(scale, scale, 1f), duration);
            AnimateVector3(_cardRootVisual, nameof(Visual.Offset), new Vector3(0f, offsetY, 0f), duration);
        }

        private void AnimateOpacity(Visual? visual, float opacity, TimeSpan duration)
        {
            if (visual == null)
            {
                return;
            }

            AnimateScalar(visual, nameof(Visual.Opacity), opacity, duration);
        }

        private void AnimateScalar(CompositionObject target, string propertyName, float value, TimeSpan duration)
        {
            if (_compositor == null)
            {
                return;
            }

            var animation = _compositor.CreateScalarKeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = duration;
            target.StartAnimation(propertyName, animation);
        }

        private void AnimateVector3(CompositionObject target, string propertyName, Vector3 value, TimeSpan duration)
        {
            if (_compositor == null)
            {
                return;
            }

            var animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.InsertKeyFrame(1f, value);
            animation.Duration = duration;
            target.StartAnimation(propertyName, animation);
        }

        private TimeSpan GetAnimationDuration(bool isActive)
        {
            var baseline = isActive ? ActiveDuration : HoverDuration;
            var factor = IsPrimaryTone ? 1.30 : 1.0;
            return TimeSpan.FromMilliseconds(baseline.TotalMilliseconds * factor);
        }

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
            double AmbientFogOpacity,
            double BloomOpacity,
            double HeroAuraOpacity,
            double HeroBandOpacity,
            double TierRailOpacity,
            double BadgeOpacity,
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
            Color IconBackgroundColor,
            Color IconBorderColor,
            Color IconForegroundColor,
            Color TitleColor,
            Color ValueColor,
            Color SubInfoColor,
            Color BadgeBackgroundColor,
            Color BadgeBorderColor,
            Color BadgeTextColor,
            double DividerOpacity,
            string TierLabel,
            Thickness ContentMargin,
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
