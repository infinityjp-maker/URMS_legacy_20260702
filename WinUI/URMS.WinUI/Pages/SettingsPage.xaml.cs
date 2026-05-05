using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using URMS.WinUI.Services;
using System;
using System.IO;
using System.Reflection;

namespace URMS.WinUI.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private bool _initializing = true;
        private readonly AppSettingsService _svc = AppSettingsService.Instance;

        public event EventHandler? CloseRequested;

        // ─── 現在アクティブなカテゴリパネル ───────────────────────────────
        private enum SettingsCategory { General, UI, System, Developer }
        private SettingsCategory _activeCategory = SettingsCategory.General;

        public SettingsPage()
        {
            this.InitializeComponent();
            InitializeSelections();

            var asm = Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            TxtVersion.Text   = ver is not null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "1.0.0";
            TxtBuildDate.Text = File.GetLastWriteTime(asm.Location).ToString("yyyy/MM/dd");
            TxtSettingsPath.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            TxtApplyStatus.Text  = "待機中";

            // Boot times
            if (App.LaunchTime != default)
                TxtBootLaunch.Text = App.LaunchTime.ToString("HH:mm:ss.fff");
            if (App.DashboardLoadTime != default)
                TxtBootDash.Text = $"{(App.DashboardLoadTime - App.LaunchTime).TotalMilliseconds:F0} ms";
            if (App.BootCompleteTime != default)
                TxtBootComplete.Text = $"{(App.BootCompleteTime - App.LaunchTime).TotalMilliseconds:F0} ms";

            _initializing = false;
        }

        // ── カテゴリ選択ハンドラ ──────────────────────────────────────────────
        private void OnCatGeneral(object sender, RoutedEventArgs e)   => SwitchCategory(SettingsCategory.General);
        private void OnCatUi(object sender, RoutedEventArgs e)        => SwitchCategory(SettingsCategory.UI);
        private void OnCatSystem(object sender, RoutedEventArgs e)    => SwitchCategory(SettingsCategory.System);
        private void OnCatDeveloper(object sender, RoutedEventArgs e) => SwitchCategory(SettingsCategory.Developer);

        private void SwitchCategory(SettingsCategory cat)
        {
            _activeCategory = cat;
            PanelGeneral.Visibility   = cat == SettingsCategory.General   ? Visibility.Visible : Visibility.Collapsed;
            PanelUI.Visibility        = cat == SettingsCategory.UI        ? Visibility.Visible : Visibility.Collapsed;
            PanelSystem.Visibility    = cat == SettingsCategory.System    ? Visibility.Visible : Visibility.Collapsed;
            PanelDeveloper.Visibility = cat == SettingsCategory.Developer ? Visibility.Visible : Visibility.Collapsed;

            BtnCatGeneral.Style   = cat == SettingsCategory.General   ? (Style)Resources["CatButtonActiveStyle"] : (Style)Resources["CatButtonStyle"];
            BtnCatUI.Style        = cat == SettingsCategory.UI        ? (Style)Resources["CatButtonActiveStyle"] : (Style)Resources["CatButtonStyle"];
            BtnCatSystem.Style    = cat == SettingsCategory.System    ? (Style)Resources["CatButtonActiveStyle"] : (Style)Resources["CatButtonStyle"];
            BtnCatDeveloper.Style = cat == SettingsCategory.Developer ? (Style)Resources["CatButtonActiveStyle"] : (Style)Resources["CatButtonStyle"];

            // SYSTEM 選択時に Boot Times を最新化
            if (cat == SettingsCategory.System)
            {
                if (App.BootCompleteTime != default)
                    TxtBootComplete.Text = $"{(App.BootCompleteTime - App.LaunchTime).TotalMilliseconds:F0} ms";
                if (App.DashboardLoadTime != default)
                    TxtBootDash.Text = $"{(App.DashboardLoadTime - App.LaunchTime).TotalMilliseconds:F0} ms";
            }
        }

        // ── 初期値設定 ─────────────────────────────────────────────────────
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

            // UI カテゴリ初期値
            SliderAnimIntensity.Value = _svc.Settings.AnimIntensity;
            TxtAnimIntensity.Text     = $"{_svc.Settings.AnimIntensity:F1}×";
            CmbTargetFps.SelectedIndex = _svc.Settings.TargetFps switch
            {
                60  => 0, 90 => 1, _ => 2   // 120
            };

            // SYSTEM カテゴリ
            TogBootAnim.IsOn = _svc.Settings.BootAnimEnabled;
        }

        // ── GENERAL ──────────────────────────────────────────────────────────
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
            _svc.Settings.Theme = theme.ToString();
            _svc.Save();
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
            _svc.Settings.Language = lang.ToString();
            _svc.Save();
            TxtApplyStatus.Text = $"言語適用: {((ComboBoxItem)CmbLanguage.SelectedItem).Content}";
        }

        // ── UI ───────────────────────────────────────────────────────────────
        private void OnAccentColorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initializing) return;
            var tag = ((ComboBoxItem?)CmbAccentColor.SelectedItem)?.Tag?.ToString() ?? "#00F7FF";
            _svc.Settings.AccentColor = tag;
            _svc.Save();
            TxtApplyStatus.Text = $"アクセントカラー: {tag}";
        }

        private void OnAnimIntensityChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing) return;
            double v = Math.Round(e.NewValue, 1);
            TxtAnimIntensity.Text      = $"{v:F1}×";
            _svc.Settings.AnimIntensity = v;
            _svc.Save();
            TxtApplyStatus.Text = $"アニメ強度: {v:F1}×";
        }

        private void OnTargetFpsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_initializing) return;
            int fps = CmbTargetFps.SelectedIndex switch { 0 => 60, 1 => 90, _ => 120 };
            _svc.Settings.TargetFps = fps;
            _svc.Save();
            TxtApplyStatus.Text = $"目標FPS: {fps}";
        }

        // ── SYSTEM ───────────────────────────────────────────────────────────
        private void OnBootAnimToggled(object sender, RoutedEventArgs e)
        {
            if (_initializing) return;
            _svc.Settings.BootAnimEnabled = TogBootAnim.IsOn;
            _svc.Save();
            TxtApplyStatus.Text = $"Boot 演出: {(TogBootAnim.IsOn ? "ON" : "OFF")} (次回起動から)";
        }

        // ── DEVELOPER ────────────────────────────────────────────────────────
        private void OnDebugLogToggled(object sender, RoutedEventArgs e)
        {
            if (_initializing) return;
            TxtApplyStatus.Text = $"デバッグログ: {(TogDebugLog.IsOn ? "ON" : "OFF")}";
        }

        // ── 戻る ─────────────────────────────────────────────────────────────
        private void OnBack(object sender, RoutedEventArgs e)
        {
            if (Frame?.CanGoBack == true) { Frame.GoBack(); return; }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
