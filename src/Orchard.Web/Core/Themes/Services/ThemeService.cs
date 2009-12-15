using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Extensions;
using Orchard.Logging;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.Core.Themes.Models;

namespace Orchard.Core.Themes.Services {
    public class ThemeService : IThemeService {
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IThemeSelector> _themeSelectors;

        public ThemeService(
            IExtensionManager extensionManager,
            IEnumerable<IThemeSelector> themeSelectors) {
            _extensionManager = extensionManager;
            _themeSelectors = themeSelectors;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public ISite CurrentSite { get; set; }

        #region Implementation of IThemeService

        public ITheme GetSiteTheme() {
            string currentThemeName = CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return GetThemeByName(currentThemeName);
        }

        public void SetSiteTheme(string themeName) {
            if (GetThemeByName(themeName) != null) {
                CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName = themeName;
            }
        }

        public ITheme GetRequestTheme(RequestContext requestContext) {

            var requestTheme = _themeSelectors
                .Select(x => x.GetTheme(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

            if (requestTheme == null) {
                return null;
            }

            return GetThemeByName(requestTheme.ThemeName);
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


        public void InstallTheme(HttpPostedFileBase file) {
            _extensionManager.InstallExtension("Theme", file);
        }

        public void UninstallTheme(string themeName) {
            _extensionManager.UninstallExtension("Theme", themeName);
        }

        #endregion
    }
}
