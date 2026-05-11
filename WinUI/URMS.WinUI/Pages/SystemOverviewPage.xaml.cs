using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;

namespace URMS.WinUI.Pages
{
    public sealed partial class SystemOverviewPage : Page
    {
        public SystemOverviewPage()
        {
            this.InitializeComponent();
            this.Loaded += (_, _) => FadeIn();
        }

        private void FadeIn()
        {
            var animation = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(320) };
            var sb = new Storyboard();
            sb.Children.Add(animation);
            Storyboard.SetTarget(animation, RootGrid);
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Begin();
        }

        private async Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var animation = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(320) };
            var sb = new Storyboard();
            sb.Children.Add(animation);
            Storyboard.SetTarget(animation, RootGrid);
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Completed += (_, _) => tcs.TrySetResult(true);
            sb.Begin();
            await tcs.Task;
        }

        private async void OnNavDashboard(object sender, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(DashboardPage)); }
        private async void OnNavWeather(object sender, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(WeatherPage)); }
        private async void OnNavFlow(object sender, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(FlowPage)); }
        private async void OnNavLog(object sender, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(LogPage)); }
        private async void OnNavSettings(object sender, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(SettingsPage)); }

        private void OnNavPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not UIElement target) return;
            var animation = new DoubleAnimation { To = 0.86, Duration = TimeSpan.FromMilliseconds(180) };
            var sb = new Storyboard();
            sb.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Begin();
        }

        private void OnNavPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not UIElement target) return;
            var animation = new DoubleAnimation { To = 1.0, Duration = TimeSpan.FromMilliseconds(180) };
            var sb = new Storyboard();
            sb.Children.Add(animation);
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");
            sb.Begin();
        }
    }
}
