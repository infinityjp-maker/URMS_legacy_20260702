using System;
using System.Collections.Generic;

namespace URMS.WinUI.Services
{
    public enum AppLanguage { Japanese, English }

    public sealed class LanguageService
    {
        public static LanguageService Instance { get; } = new();
        private LanguageService() { }

        public AppLanguage Current { get; private set; } = AppLanguage.Japanese;
        public event EventHandler? LanguageChanged;

        private static readonly Dictionary<string, string[]> _table = new()
        {
            // [0]=Japanese  [1]=English
            { "CardSchedule",  new[] { "SCHEDULE",  "SCHEDULE"  } },
            { "CardWeather",   new[] { "WEATHER",   "WEATHER"   } },
            { "CardTask",      new[] { "TASKS",     "TASKS"     } },
            { "CardLauncher",  new[] { "LAUNCHER",  "LAUNCHER"  } },
            { "CardNetwork",   new[] { "NETWORK",   "NETWORK"   } },
            { "CardSystem",    new[] { "SYSTEM",    "SYSTEM"    } },
            { "CardCiCd",      new[] { "CI/CD",     "CI/CD"     } },
        };

        public string Get(string key)
        {
            if (_table.TryGetValue(key, out var v))
                return Current == AppLanguage.Japanese ? v[0] : v[1];
            return key;
        }

        public void Apply(AppLanguage lang)
        {
            if (Current == lang) return;
            Current = lang;
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
