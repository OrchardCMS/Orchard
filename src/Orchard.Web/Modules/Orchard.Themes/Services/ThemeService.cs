using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Environment.Descriptor;
using Orchard.Environment.Descriptor.Models;
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
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IShellDescriptorManager _shellDescriptorManager;

        public ThemeService(
            IShellDescriptorManager shellDescriptorManager,
            IExtensionManager extensionManager,
            IEnumerable<IThemeSelector> themeSelectors,
            IModuleService moduleService,
            IWorkContextAccessor workContextAccessor,
            ShellDescriptor shellDescriptor) {
            _shellDescriptorManager = shellDescriptorManager;
            _extensionManager = extensionManager;
            _themeSelectors = themeSelectors;
            _moduleService = moduleService;
            _workContextAccessor = workContextAccessor;
            _shellDescriptor = shellDescriptor;
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
            if (DoEnableTheme(themeName)) {
                CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName = themeName;
            }
        }

        public void EnableTheme(string themeName) {
            DoEnableTheme(themeName);
        }

        public void DisableTheme(string themeName) {
            DisableThemeFeatures(themeName);
        }

        private bool AllBaseThemesAreInstalled(string baseThemeName) {
            var themesSeen = new List<string>();
            while (!string.IsNullOrWhiteSpace(baseThemeName)) {
                //todo: (heskew) need a better way to protect from recursive references
                if (themesSeen.Contains(baseThemeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" was already seen - looks like we're going around in circles.", baseThemeName).Text);
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
                var theme = GetThemeByName(themeName);
                if (theme == null)
                    break;
                themes.Enqueue(themeName);

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
                _moduleService.EnableFeatures(new[] {themes.Pop()});
        }

        private bool DoEnableTheme(string themeName) {
            if (string.IsNullOrWhiteSpace(themeName))
                return false;

            //todo: (heskew) need messages given in addition to all of these early returns so something meaningful can be presented to the user
            var themeToEnable = GetThemeByName(themeName);
            if (themeToEnable == null)
                return false;

            // ensure all base themes down the line are present and accounted for
            //todo: (heskew) dito on the need of a meaningful message
            if (!AllBaseThemesAreInstalled(themeToEnable.BaseTheme))
                return false;

            // enable all theme features
            EnableThemeFeatures(themeToEnable.ThemeName);
            return true;
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
        /// Loads only installed themes
        /// </summary>
        public IEnumerable<ITheme> GetInstalledThemes() {
            return GetThemes(_extensionManager.AvailableExtensions());
        }

        /// <summary>
        /// Loads only enabled themes
        /// </summary>
        public IEnumerable<ITheme> GetEnabledThemes() {
            return GetThemes(_extensionManager.EnabledExtensions(_shellDescriptor));
        }

        private IEnumerable<ITheme> GetThemes(IEnumerable<ExtensionDescriptor> extensions) {
            var themes = new List<ITheme>();
            foreach (var descriptor in extensions) {

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

        private bool IsThemeEnabled(ExtensionDescriptor descriptor) {
            return _shellDescriptorManager.GetShellDescriptor().Features.Any(sf => sf.Name == descriptor.Name);
        }

        private ITheme CreateTheme(ExtensionDescriptor descriptor) {

            var localizer = LocalizationUtilities.Resolve(_workContextAccessor.GetContext(), String.Concat(descriptor.Location, "/", descriptor.Name, "/Theme.txt"));

            return new Theme {
                Author = TryLocalize("Author", descriptor.Author, localizer) ?? "",
                Description = TryLocalize("Description", descriptor.Description, localizer) ?? "",
                DisplayName = TryLocalize("Name", descriptor.DisplayName, localizer) ?? "",
                HomePage = TryLocalize("Website", descriptor.WebSite, localizer) ?? "",
                ThemeName = descriptor.Name,
                Version = descriptor.Version ?? "",
                Tags = TryLocalize("Tags", descriptor.Tags, localizer) ?? "",
                Zones = descriptor.Zones ?? "",
                BaseTheme = descriptor.BaseTheme ?? "",
                Enabled = IsThemeEnabled(descriptor)
            };
        }
    }
}