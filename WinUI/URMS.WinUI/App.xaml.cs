using Microsoft.UI.Xaml;
using System;
using System.IO;

namespace URMS.WinUI
{
    public partial class App : Application
    {
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
            e.Handled = true;  // レイアウトサイクルなどの非致命的な例外を抑止して起動持続
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();
            window.Activate();
        }
    }
}
