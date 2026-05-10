using System;

namespace URMS.WinUI.Services
{
    public enum AppTheme { Futuristic, Dark, Light }

    public sealed class ThemeService
    {
        public static ThemeService Instance { get; } = new();
        private ThemeService() { }

        public AppTheme Current { get; private set; } = AppTheme.Futuristic;
        public event EventHandler? ThemeChanged;

        public void Apply(AppTheme theme)
        {
            if (Current == theme) return;
            Current = theme;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
