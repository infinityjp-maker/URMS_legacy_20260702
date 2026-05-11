using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Threading.Tasks;

namespace URMS.WinUI.Pages
{
    public sealed partial class FlowPage : Page
    {
        public FlowPage()
        {
            this.InitializeComponent();
            this.Loaded += (_, _) => FadeIn();
        }

        private void FadeIn()
        {
            var a = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromMilliseconds(320) };
            var sb = new Storyboard(); sb.Children.Add(a);
            Storyboard.SetTarget(a, RootGrid); Storyboard.SetTargetProperty(a, "Opacity"); sb.Begin();
        }

        private async Task FadeOutAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var a = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromMilliseconds(320) };
            var sb = new Storyboard(); sb.Children.Add(a);
            Storyboard.SetTarget(a, RootGrid); Storyboard.SetTargetProperty(a, "Opacity");
            sb.Completed += (_, _) => tcs.TrySetResult(true); sb.Begin(); await tcs.Task;
        }

        private async void OnNavDashboard(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(DashboardPage)); }
        private async void OnNavWeather(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(WeatherPage)); }
        private async void OnNavSystem(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(SystemOverviewPage)); }
        private async void OnNavLog(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(LogPage)); }
        private async void OnNavSettings(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(SettingsPage)); }

        private void OnNavPointerEntered(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 0.86, 180);
        private void OnNavPointerExited(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 1.0, 180);
        private void OnNodePointerEntered(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 0.84, 250);
        private void OnNodePointerExited(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 1.0, 250);

        private void FadeOpacity(UIElement? target, double to, int ms)
        {
            if (target == null) return;
            var a = new DoubleAnimation { To = to, Duration = TimeSpan.FromMilliseconds(ms) };
            var sb = new Storyboard(); sb.Children.Add(a);
            Storyboard.SetTarget(a, target); Storyboard.SetTargetProperty(a, "Opacity"); sb.Begin();
        }

        private async void OnNodeTapped(object sender, TappedRoutedEventArgs e)
        {
            var name = (sender as FrameworkElement)?.Name ?? "Node";
            var dlg = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = $"Flow detail: {name}",
                Content = "Node execution details, recent logs, and remediation action points.",
                PrimaryButtonText = "Close"
            };
            await dlg.ShowAsync();
        }
    }
}
