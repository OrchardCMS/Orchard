using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
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
        private IWorkContextAccessor _workContextAccessor;

        public ThemeService(
            IExtensionManager extensionManager,
            IEnumerable<IThemeSelector> themeSelectors,
            IModuleService moduleService,
            IWorkContextAccessor workContextAccessor) {
            _extensionManager = extensionManager;
            _themeSelectors = themeSelectors;
            _moduleService = moduleService;
            _workContextAccessor = workContextAccessor;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
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

            // ensure all base themes down the line are present and accounted for
            //todo: (heskew) dito on the need of a meaningful message
            if (!AllBaseThemesAreInstalled(themeToSet.BaseTheme))
                return;

            // disable all theme features
            DisableThemeFeatures(CurrentSite.As<ThemeSiteSettingsPart>().CurrentThemeName);

            // enable all theme features
            EnableThemeFeatures(themeToSet.ThemeName);

            CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName = themeToSet.ThemeName;
        }

        private bool AllBaseThemesAreInstalled(string baseThemeName) {
            var themesSeen = new List<string>();
            while (!string.IsNullOrWhiteSpace(baseThemeName)) {
                //todo: (heskew) need a better way to protect from recursive references
                if (themesSeen.Contains(baseThemeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" was already seen - looks like we're going aruond in circles.", baseThemeName).Text);
                themesSeen.Add(baseThemeName);

                var baseTheme = GetThemeByName(baseThemeName);
                if (baseTheme == null)
                    return false;
                baseThemeName = baseTheme.BaseTheme;
            }

            return true;
        }

        private void DisableThemeFeatures(string themeName) {
            var themes = new Queue<string>();
            while (themeName != null) {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features disabled.", themeName).Text);
                themes.Enqueue(themeName);

                var theme = GetThemeByName(themeName);
                themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
                    ? theme.BaseTheme
                    : null;
            }

            while (themes.Count > 0)
                _moduleService.DisableFeatures(new[] { themes.Dequeue() });
        }

        private void EnableThemeFeatures(string themeName) {
            var themes = new Stack<string>();
            while(themeName != null) {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features enabled.", themeName).Text);
                themes.Push(themeName);

                var theme = GetThemeByName(themeName);
                themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
                    ? theme.BaseTheme
                    : null;
            }

            while (themes.Count > 0)
                _moduleService.DisableFeatures(new[] {themes.Pop()});
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

        private static string TryLocalize(string key, string original, Localizer localizer) {
            var localized = localizer(key).Text;

            if ( key == localized ) {
                // no specific localization available
                return original;
            }

            return localized;
        }

        private ITheme CreateTheme(ExtensionDescriptor descriptor) {

            var localizer = LocalizationUtilities.Resolve(_workContextAccessor.GetContext(), String.Concat(descriptor.Location, "/", descriptor.Name, "/Theme.txt"));

            return new Theme {
                Author = TryLocalize("Author", descriptor.Author, localizer) ?? "",
                Description = TryLocalize("Description", descriptor.Description, localizer) ?? "",
                DisplayName = TryLocalize("DisplayName", descriptor.DisplayName, localizer) ?? "",
                HomePage = TryLocalize("Website", descriptor.WebSite, localizer) ?? "",
                ThemeName = descriptor.Name,
                Version = descriptor.Version ?? "",
                Tags = TryLocalize("Tags", descriptor.Tags, localizer) ?? "",
                Zones = descriptor.Zones ?? "",
                BaseTheme = descriptor.BaseTheme ?? "",
            };
        }
    }
}