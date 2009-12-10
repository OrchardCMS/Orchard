using System;
using System.Collections.Generic;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Core.Themes.Models;

namespace Orchard.Core.Themes.Services {
    public class ThemeService : IThemeService {
        public ThemeService() {
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
            throw new NotImplementedException();
        }

        public IEnumerable<ITheme> GetInstalledThemes() {
            throw new NotImplementedException();
        }

        #endregion
    }
}
