using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Modules;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    [UsedImplicitly]
    public class ThemeService : IThemeService {
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IThemeSelector> _themeSelectors;
        private readonly IModuleService _moduleService;

        public ThemeService(
            IExtensionManager extensionManager,
            IEnumerable<IThemeSelector> themeSelectors,
            IModuleService moduleService) {
            _extensionManager = extensionManager;
            _themeSelectors = themeSelectors;
            _moduleService = moduleService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ITheme GetSiteTheme() {
            string currentThemeName = CurrentSite.As<ThemeSiteSettingsPart>().CurrentThemeName;

            if (string.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return GetThemeByName(currentThemeName);
        }

        public void SetSiteTheme(string themeName) {
            if (string.IsNullOrWhiteSpace(themeName))
                return;

            //todo: (heskew) need messages given in addition to all of these early returns so something meaningful can be presented to the user
            var themeToSet = GetThemeByName(themeName);
            if (themeToSet == null)
                return;

            //if there's a base theme, it needs to be present
            ITheme baseTheme = null;
            if (!string.IsNullOrWhiteSpace(themeToSet.BaseTheme)) {
                baseTheme = GetThemeByName(themeToSet.BaseTheme);
                if (baseTheme == null)
                    return;
            }

            var currentTheme = CurrentSite.As<ThemeSiteSettingsPart>().CurrentThemeName;
            if ( !string.IsNullOrEmpty(currentTheme) )
                _moduleService.DisableFeatures(new[] {currentTheme}, true);

            if (baseTheme != null)
                _moduleService.EnableFeatures(new[] {themeToSet.BaseTheme});

            _moduleService.EnableFeatures(new[] {themeToSet.ThemeName}, true);

            CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName = themeToSet.ThemeName;
        }

        public ITheme GetRequestTheme(RequestContext requestContext) {
            var requestTheme = _themeSelectors
                .Select(x => x.GetTheme(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority);

            if (requestTheme.Count() < 1)
                return null;

            foreach (var theme in requestTheme) {
                var t = GetThemeByName(theme.ThemeName);
                if (t != null)
                    return t;
            }

            return null;
        }

        public ITheme GetThemeByName(string name) {
            foreach (var descriptor in _extensionManager.AvailableExtensions()) {
                if (string.Equals(descriptor.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return CreateTheme(descriptor);
                }
            }
            return null;
        }

        /// <summary>
        /// Loads only enabled themes
        /// </summary>
        public IEnumerable<ITheme> GetInstalledThemes() {
            var themes = new List<ITheme>();
            foreach (var descriptor in _extensionManager.AvailableExtensions()) {

                if (!string.Equals(descriptor.ExtensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                ITheme theme = CreateTheme(descriptor);

                if (!theme.Tags.Contains("hidden")) {
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

        private static ITheme CreateTheme(ExtensionDescriptor descriptor) {
            return new Theme {
                Author = descriptor.Author ?? "",
                Description = descriptor.Description ?? "",
                DisplayName = descriptor.DisplayName ?? "",
                HomePage = descriptor.WebSite ?? "",
                ThemeName = descriptor.Name,
                Version = descriptor.Version ?? "",
                Tags = descriptor.Tags ?? "",
                Zones = descriptor.Zones ?? "",
                BaseTheme = descriptor.BaseTheme ?? "",
            };
        }
    }
}