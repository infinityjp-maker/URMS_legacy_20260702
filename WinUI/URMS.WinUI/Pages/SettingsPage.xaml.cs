using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using URMS.WinUI.Services;
using System;
using System.Reflection;

namespace URMS.WinUI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private bool _initializing = true;
        public event EventHandler? CloseRequested;

        public SettingsPage()
        {
            this.InitializeComponent();
            InitializeSelections();

            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            TxtVersion.Text = ver is not null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            TxtBuildDate.Text = System.IO.File.GetLastWriteTime(asm.Location).ToString("yyyy/MM/dd");
            TxtApplyStatus.Text = "待機中";

            _initializing = false;
        }

        private void InitializeSelections()
        {
            CmbTheme.SelectedIndex = ThemeService.Instance.Current switch
            {
                AppTheme.Futuristic => 0,
                AppTheme.Dark       => 1,
                AppTheme.Light      => 2,
                _                   => 0,
            };

            CmbLanguage.SelectedIndex = LanguageService.Instance.Current switch
            {
                AppLanguage.Japanese => 0,
                AppLanguage.English  => 1,
                _                   => 0,
            };
        }

        private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initializing) return;
            var theme = CmbTheme.SelectedIndex switch
            {
                0 => AppTheme.Futuristic,
                1 => AppTheme.Dark,
                2 => AppTheme.Light,
                _ => AppTheme.Futuristic,
            };
            ThemeService.Instance.Apply(theme);
            TxtApplyStatus.Text = $"テーマ適用: {((ComboBoxItem)CmbTheme.SelectedItem).Content}";
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initializing) return;
            var lang = CmbLanguage.SelectedIndex switch
            {
                0 => AppLanguage.Japanese,
                1 => AppLanguage.English,
                _ => AppLanguage.Japanese,
            };
            LanguageService.Instance.Apply(lang);
            TxtApplyStatus.Text = $"言語適用: {((ComboBoxItem)CmbLanguage.SelectedItem).Content}";
        }

        private void OnBack(object sender, RoutedEventArgs e)
        {
            if (Frame?.CanGoBack == true)
            {
                Frame.GoBack();
                return;
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
