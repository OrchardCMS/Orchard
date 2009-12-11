using System;
using System.Collections.Generic;
using Orchard.Extensions;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Core.Themes.Models;

namespace Orchard.Core.Themes.Services {
    public class ThemeService : IThemeService {
        private readonly IExtensionManager _extensionManager;

        public ThemeService(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }

        #region Implementation of IThemeService

        public ITheme GetCurrentTheme() {
            string currentThemeName = CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return GetThemeByName(currentThemeName);
        }

        public ITheme GetThemeByName(string name) {
            foreach (var descriptor in _extensionManager.AvailableExtensions()) {
                if (String.Equals(descriptor.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return new Theme {
                        Author = descriptor.Author ?? String.Empty,
                        Description = descriptor.Description ?? String.Empty,
                        DisplayName = descriptor.DisplayName ?? String.Empty,
                        HomePage = descriptor.HomePage ?? String.Empty,
                        ThemeName = descriptor.Name,
                        Version = descriptor.Version ?? String.Empty,
                    };
                }
            }
            return null;
        }

        public IEnumerable<ITheme> GetInstalledThemes() {
            List<ITheme> themes = new List<ITheme>();
            foreach (var descriptor in _extensionManager.AvailableExtensions()) {
                if (String.Equals(descriptor.ExtensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                    Theme theme = new Theme {
                        Author = descriptor.Author ?? String.Empty,
                        Description = descriptor.Description ?? String.Empty,
                        DisplayName = descriptor.DisplayName ?? String.Empty,
                        HomePage = descriptor.HomePage ?? String.Empty,
                        ThemeName = descriptor.Name,
                        Version = descriptor.Version ?? String.Empty,
                    };
                    themes.Add(theme);
                }
            }
            return themes;
        }

        public void SetCurrentTheme(string themeName) {
            if (GetThemeByName(themeName) != null) {
                CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName = themeName;
            }
        }

        #endregion
    }
}
