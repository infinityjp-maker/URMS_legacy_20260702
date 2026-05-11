using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using URMS.WinUI.Services;
using System;
using System.Threading.Tasks;

namespace URMS.WinUI.Pages
{
    public sealed partial class WeatherPage : Page
    {
        private readonly LanguageService _lang = LanguageService.Instance;

        public WeatherPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Apply();
            StartPageFadeIn();
        }

        private void Apply()
        {
            // Static placeholder data — replace with live weather API in future
            TxtMainTemp.Text     = "22\u00B0C";
            TxtMainDesc.Text     = "Current Conditions";
            TxtMainLocation.Text = "Tokyo, Minato-ku";
            TxtHumidity.Text     = "68%";
            TxtWind.Text         = "12 km/h";
            TxtFeelsLike.Text    = "20\u00B0C";
            TxtUvIndex.Text      = "4";
            TxtTempHigh.Text     = "26\u00B0C";
            TxtTempLow.Text      = "17\u00B0C";
            WxMainAnim.Condition = "partly_cloudy";
            WxHeaderAnim.Condition = "partly_cloudy";
            TxtLastUpdated.Text = $"Last updated: {DateTime.Now:HH:mm}";
        }

        private async void OnNavDashboard(object sender, RoutedEventArgs e)
        {
            await FadeOutAsync();
            if (Frame.CanGoBack)
                Frame.GoBack();
            else
                Frame.Navigate(typeof(DashboardPage));
        }

        private void StartPageFadeIn()
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(320)
            };
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, RootGrid);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Begin();
        }

        private Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(320)
            };
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, RootGrid);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Completed += (_, _) => tcs.TrySetResult(true);
            storyboard.Begin();
            return tcs.Task;
        }

        private void OnCityCardClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string city)
                return;

            switch (city)
            {
                case "New York":
                    TxtMainLocation.Text = "New York, Manhattan";
                    TxtMainTemp.Text = "14\u00B0C";
                    TxtMainDesc.Text = "Current Conditions";
                    TxtWind.Text = "18 km/h";
                    TxtHumidity.Text = "59%";
                    TxtUvIndex.Text = "3";
                    TxtTempHigh.Text = "16\u00B0C";
                    TxtTempLow.Text = "9\u00B0C";
                    TxtFeelsLike.Text = "12\u00B0C";
                    WxMainAnim.Condition = "cloudy";
                    break;
                case "California":
                    TxtMainLocation.Text = "California, San Jose";
                    TxtMainTemp.Text = "26\u00B0C";
                    TxtMainDesc.Text = "Current Conditions";
                    TxtWind.Text = "10 km/h";
                    TxtHumidity.Text = "42%";
                    TxtUvIndex.Text = "7";
                    TxtTempHigh.Text = "29\u00B0C";
                    TxtTempLow.Text = "19\u00B0C";
                    TxtFeelsLike.Text = "27\u00B0C";
                    WxMainAnim.Condition = "sunny";
                    break;
            }
        }

        private void OnWorldCityClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string city)
                return;

            switch (city)
            {
                case "Tokyo":
                    TxtMainLocation.Text = "Tokyo, Minato-ku";
                    TxtMainTemp.Text = "22\u00B0C";
                    TxtMainDesc.Text = "Current Conditions";
                    TxtWind.Text = "12 km/h";
                    TxtHumidity.Text = "68%";
                    TxtUvIndex.Text = "4";
                    TxtTempHigh.Text = "26\u00B0C";
                    TxtTempLow.Text = "17\u00B0C";
                    TxtFeelsLike.Text = "20\u00B0C";
                    WxMainAnim.Condition = "partly_cloudy";
                    break;
                case "Seoul":
                    TxtMainLocation.Text = "Seoul, Yongsan";
                    TxtMainTemp.Text = "19\u00B0C";
                    TxtMainDesc.Text = "Current Conditions";
                    TxtWind.Text = "15 km/h";
                    TxtHumidity.Text = "64%";
                    TxtUvIndex.Text = "3";
                    TxtTempHigh.Text = "21\u00B0C";
                    TxtTempLow.Text = "14\u00B0C";
                    TxtFeelsLike.Text = "18\u00B0C";
                    WxMainAnim.Condition = "cloudy";
                    break;
                case "Singapore":
                    TxtMainLocation.Text = "Singapore";
                    TxtMainTemp.Text = "31\u00B0C";
                    TxtMainDesc.Text = "Current Conditions";
                    TxtWind.Text = "8 km/h";
                    TxtHumidity.Text = "79%";
                    TxtUvIndex.Text = "8";
                    TxtTempHigh.Text = "33\u00B0C";
                    TxtTempLow.Text = "27\u00B0C";
                    TxtFeelsLike.Text = "34\u00B0C";
                    WxMainAnim.Condition = "sunny";
                    break;
            }

            AnimateOverviewPoint();
        }

        private async void OnForecastRowClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string day)
                return;

            var dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = $"Forecast detail: {day}",
                Content = "Precipitation risk low. Wind remains stable. No operational weather alerts.",
                PrimaryButtonText = "Close"
            };

            await dialog.ShowAsync();
        }

        private void OnToggleForecastRange(object sender, RoutedEventArgs e)
        {
            bool showSevenDays = ForecastRangeToggle.IsOn;
            ForecastRow6.Visibility = showSevenDays ? Visibility.Visible : Visibility.Collapsed;
            ForecastRow7.Visibility = showSevenDays ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCardPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not UIElement target)
                return;

            var animation = new DoubleAnimation
            {
                To = 0.86,
                Duration = TimeSpan.FromMilliseconds(180)
            };
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Begin();
        }

        private void OnCardPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not UIElement target)
                return;

            var animation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(180)
            };
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Begin();
        }

        private void AnimateOverviewPoint()
        {
            var pointAnim = new DoubleAnimation
            {
                To = 350,
                Duration = TimeSpan.FromMilliseconds(250),
                AutoReverse = true
            };

            var fillAnim = new DoubleAnimation
            {
                To = 940,
                Duration = TimeSpan.FromMilliseconds(250),
                AutoReverse = true
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(pointAnim);
            storyboard.Children.Add(fillAnim);

            Storyboard.SetTarget(pointAnim, WxOverviewLine);
            Storyboard.SetTargetProperty(pointAnim, "(Canvas.Left)");
            Storyboard.SetTarget(fillAnim, WxOverviewFill);
            Storyboard.SetTargetProperty(fillAnim, "Width");

            storyboard.Begin();
        }

        private async void OnNavSystem(object sender, RoutedEventArgs e)
        {
            await FadeOutAsync();
            Frame.Navigate(typeof(SystemOverviewPage));
        }

        private async void OnNavFlow(object sender, RoutedEventArgs e)
        {
            await FadeOutAsync();
            Frame.Navigate(typeof(FlowPage));
        }

        private async void OnNavLog(object sender, RoutedEventArgs e)
        {
            await FadeOutAsync();
            Frame.Navigate(typeof(LogPage));
        }

        private async void OnNavSettings(object sender, RoutedEventArgs e)
        {
            await FadeOutAsync();
            Frame.Navigate(typeof(SettingsPage));
        }

        private void OnNavPointerEntered(object sender, PointerRoutedEventArgs e)
            => OnCardPointerEntered(sender, e);

        private void OnNavPointerExited(object sender, PointerRoutedEventArgs e)
            => OnCardPointerExited(sender, e);
    }
}
