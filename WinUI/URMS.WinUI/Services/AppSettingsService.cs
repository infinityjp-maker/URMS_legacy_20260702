using System;
using System.IO;
using System.Text.Json;

namespace URMS.WinUI.Services
{
    /// <summary>ユーザー設定 DTO（settings.json に保存される）</summary>
    public sealed class AppSettingsData
    {
        public string  AccentColor       { get; set; } = "#00F7FF";
        public double  AnimIntensity     { get; set; } = 1.0;   // 0.0 ~ 2.0
        public bool    BootAnimEnabled   { get; set; } = true;
        public int     TargetFps         { get; set; } = 120;
        public string  Theme             { get; set; } = "Futuristic";
        public string  Language          { get; set; } = "Japanese";
    }

    /// <summary>設定サービス（シングルトン）</summary>
    public sealed class AppSettingsService
    {
        public static AppSettingsService Instance { get; } = new();
        private AppSettingsService() { Load(); }

        private static readonly string _settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public AppSettingsData Settings { get; private set; } = new();

        public event EventHandler? SettingsChanged;

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(Settings,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch { /* 保存失敗は無視 */ }
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return;
                var json = File.ReadAllText(_settingsPath);
                var data = JsonSerializer.Deserialize<AppSettingsData>(json);
                if (data is not null)
                    Settings = data;
            }
            catch { /* ロード失敗は既定値のまま */ }
        }
    }
}
