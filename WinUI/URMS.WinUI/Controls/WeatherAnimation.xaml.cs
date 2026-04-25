using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace URMS.WinUI.Controls
{
    /// <summary>
    /// 天気種別に応じたアニメーション付きアイコンを表示するコントロール。
    /// Condition プロパティに "sunny" / "partly_cloudy" / "cloudy" /
    ///   "rainy" / "snowy" / "night" を設定する。
    /// </summary>
    public sealed partial class WeatherAnimation : UserControl
    {
        public static readonly DependencyProperty ConditionProperty =
            DependencyProperty.Register(nameof(Condition), typeof(string),
                typeof(WeatherAnimation),
                new PropertyMetadata("sunny", OnConditionChanged));

        public string Condition
        {
            get => (string)GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        public WeatherAnimation()
        {
            this.InitializeComponent();
            this.Loaded += (_, _) => ApplyCondition(Condition);
        }

        private static void OnConditionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WeatherAnimation wa)
                wa.ApplyCondition((string)e.NewValue);
        }

        private void ApplyCondition(string cond)
        {
            // 全 Canvas を非表示にして止める
            StopAll();

            switch (cond?.ToLowerInvariant())
            {
                case "sunny":
                    SunCanvas.Visibility = Visibility.Visible;
                    SunAnim.Begin();
                    break;

                case "partly_cloudy":
                    PartlyCloudyCanvas.Visibility = Visibility.Visible;
                    PCCloudAnim.Begin();
                    break;

                case "cloudy":
                    CloudyCanvas.Visibility = Visibility.Visible;
                    CloudyAnim.Begin();
                    break;

                case "rainy":
                    RainyCanvas.Visibility = Visibility.Visible;
                    RainAnim.Begin();
                    break;

                case "snowy":
                    SnowyCanvas.Visibility = Visibility.Visible;
                    SnowAnim.Begin();
                    break;

                case "night":
                    NightCanvas.Visibility = Visibility.Visible;
                    NightAnim.Begin();
                    break;

                default:
                    SunCanvas.Visibility = Visibility.Visible;
                    SunAnim.Begin();
                    break;
            }
        }

        private void StopAll()
        {
            SunCanvas.Visibility          = Visibility.Collapsed;
            PartlyCloudyCanvas.Visibility = Visibility.Collapsed;
            CloudyCanvas.Visibility       = Visibility.Collapsed;
            RainyCanvas.Visibility        = Visibility.Collapsed;
            SnowyCanvas.Visibility        = Visibility.Collapsed;
            NightCanvas.Visibility        = Visibility.Collapsed;

            SunAnim.Stop();
            PCCloudAnim.Stop();
            CloudyAnim.Stop();
            RainAnim.Stop();
            SnowAnim.Stop();
            NightAnim.Stop();
        }
    }
}
