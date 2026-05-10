using Microsoft.UI.Xaml;
using System;
using System.IO;
using URMS.WinUI.Pages;

namespace URMS.WinUI
{
    public partial class App : Application
    {
        // ─── 起動時間計測（フェーズ4） ───────────────────────────────────────
        public static DateTime LaunchTime        { get; private set; }
        public static DateTime DashboardLoadTime { get; private set; }
        public static DateTime BootCompleteTime  { get; private set; }

        private static readonly string LogDir  =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static readonly string LogPath =
            Path.Combine(LogDir, "boot-times.log");

        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
                File.WriteAllText(logPath,
                    $"Time: {DateTime.Now}\n" +
                    $"Type: {e.Exception?.GetType()?.FullName}\n" +
                    $"Message: {e.Exception?.Message}\n" +
                    $"Stack: {e.Exception?.StackTrace}\n" +
                    $"Inner: {e.Exception?.InnerException?.Message}\n");
            }
            catch { }
            e.Handled = true;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            LaunchTime = DateTime.Now;

            var window = new MainWindow();

            // Boot overlay を追加（フェーズ1）
            var boot = new BootHudPage();
            boot.TransitionComplete = () =>
            {
                window.RemoveBootOverlay(boot);
            };
            window.AddBootOverlay(boot);

            window.Activate();
        }

        // ─── Dashboard Loaded 時刻を記録（DashboardPage.xaml.cs から呼ぶ） ───
        public static void RecordDashboardLoaded()
        {
            DashboardLoadTime = DateTime.Now;
            AppendBootLog($"dashboard_loaded,{(DashboardLoadTime - LaunchTime).TotalMilliseconds:F0}");
        }

        // ─── Boot → Dashboard 遷移完了を記録（BootHudPage.xaml.cs から呼ぶ） ───
        public static void RecordBootTransitionComplete()
        {
            BootCompleteTime = DateTime.Now;
            AppendBootLog($"boot_complete,{(BootCompleteTime - LaunchTime).TotalMilliseconds:F0}");
        }

        private static void AppendBootLog(string entry)
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                File.AppendAllText(LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}\t{entry}{Environment.NewLine}");
            }
            catch { }
        }
    }
}
