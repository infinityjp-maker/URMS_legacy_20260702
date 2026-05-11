using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace URMS.WinUI.Pages
{
    public sealed partial class LogPage : Page
    {
        private readonly List<string> _allLogs =
        [
            "INFO  Core API started",
            "WARN  Workflow node waiting dependency",
            "ERROR Deploy pipeline timeout",
            "INFO  Cache refresh completed",
            "WARN  NAS endpoint unreachable"
        ];

        public LogPage()
        {
            this.InitializeComponent();
            this.Loaded += (_, _) =>
            {
                FadeIn();
                BindLogs(_allLogs);
            };
        }

        private void BindLogs(IEnumerable<string> logs)
        {
            LogList.Items.Clear();
            foreach (var line in logs)
            {
                var tb = new TextBlock
                {
                    Text = line,
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 11,
                    Foreground = GetLevelBrush(line),
                    Margin = new Thickness(0, 0, 0, 1)
                };
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(255, 6, 12, 20)),
                    Padding = new Thickness(8, 6, 8, 6),
                    Child = tb
                };
                LogList.Items.Add(border);
            }

            // Auto-scroll ON: latest row tracking
            if (LogList.Items.Count > 0)
                LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
        }

        private static Brush GetLevelBrush(string line)
        {
            if (line.StartsWith("ERROR")) return new SolidColorBrush(Color.FromArgb(255, 213, 156, 166));
            if (line.StartsWith("WARN")) return new SolidColorBrush(Color.FromArgb(255, 255, 178, 58));
            return new SolidColorBrush(Color.FromArgb(255, 91, 208, 255));
        }

        private void OnLogSearchChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = LogSearchBox.Text?.Trim() ?? string.Empty;
            var filtered = string.IsNullOrEmpty(keyword)
                ? _allLogs
                : _allLogs.Where(l => l.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
            BindLogs(filtered);
        }

        private async void OnLogItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not Border border || border.Child is not TextBlock tb)
                return;

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Log detail",
                Content = tb.Text,
                PrimaryButtonText = "Close"
            };
            await dialog.ShowAsync();
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
        private async void OnNavFlow(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(FlowPage)); }
        private async void OnNavSettings(object s, RoutedEventArgs e) { await FadeOutAsync(); Frame.Navigate(typeof(SettingsPage)); }

        private void OnNavPointerEntered(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 0.86, 180);
        private void OnNavPointerExited(object sender, PointerRoutedEventArgs e) => FadeOpacity(sender as UIElement, 1.0, 180);

        private void FadeOpacity(UIElement? target, double to, int ms)
        {
            if (target == null) return;
            var a = new DoubleAnimation { To = to, Duration = TimeSpan.FromMilliseconds(ms) };
            var sb = new Storyboard(); sb.Children.Add(a);
            Storyboard.SetTarget(a, target); Storyboard.SetTargetProperty(a, "Opacity"); sb.Begin();
        }
    }
}
