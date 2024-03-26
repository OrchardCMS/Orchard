using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

namespace Orchard.Themes.Services {
    public class ThemeService : IThemeService {
        private readonly IExtensionManager _extensionManager;
        private readonly IFeatureManager _featureManager;
        private readonly IEnumerable<IThemeSelector> _themeSelectors;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ICacheManager _cacheManager;
        private readonly ISiteThemeService _siteThemeService;

        public ThemeService(
            IOrchardServices orchardServices,
            IExtensionManager extensionManager,
            IFeatureManager featureManager,
            IEnumerable<IThemeSelector> themeSelectors,
            IVirtualPathProvider virtualPathProvider,
            ICacheManager cacheManager,
            ISiteThemeService siteThemeService) {

            Services = orchardServices;

            _extensionManager = extensionManager;
            _featureManager = featureManager;
            _themeSelectors = themeSelectors;
            _virtualPathProvider = virtualPathProvider;
            _cacheManager = cacheManager;
            _siteThemeService = siteThemeService;

            if (_featureManager.FeatureDependencyNotification == null) {
                _featureManager.FeatureDependencyNotification = GenerateWarning;
            }

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void DisableThemeFeatures(string themeName) {
            var themes = new Queue<string>();
            while (themeName != null) {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features disabled.", themeName).Text);
                var theme = _extensionManager.GetExtension(themeName);
                if (theme == null)
                    break;
                themes.Enqueue(themeName);

                themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
                    ? theme.BaseTheme
                    : null;
            }

            var currentTheme = _siteThemeService.GetCurrentThemeName();

            while (themes.Count > 0) {
                var themeId = themes.Dequeue();

                // Not disabling base theme if it's the current theme.
                if (themeId != currentTheme) {
                    _featureManager.DisableFeatures(new[] { themeId });
                }
            }
        }

        public void EnableThemeFeatures(string themeName) {
            var themes = new Stack<string>();
            while(themeName != null) {
                if (themes.Contains(themeName))
                    throw new InvalidOperationException(T("The theme \"{0}\" is already in the stack of themes that need features enabled.", themeName).Text);
                themes.Push(themeName);

                var theme = _extensionManager.GetExtension(themeName);
                themeName = !string.IsNullOrWhiteSpace(theme.BaseTheme)
                    ? theme.BaseTheme
                    : null;
            }

            while (themes.Count > 0) {
                var themeId = themes.Pop();
                foreach (var featureId in _featureManager.EnableFeatures(new[] { themeId }, true)) {
                    if (themeId != featureId) {
                        var featureName = _featureManager.GetAvailableFeatures().First(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                        Services.Notifier.Information(T("{0} was enabled", featureName));
                    }
                }
            }
        }

        public ExtensionDescriptor GetRequestTheme(RequestContext requestContext) {
            var requestTheme = _themeSelectors
                .Select(x => x.GetTheme(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority).ToList();

            if (!requestTheme.Any())
                return null;

            foreach (var theme in requestTheme) {
                var t = _extensionManager.GetExtension(theme.ThemeName);
                if (t != null)
                    return t;
            }

            return _extensionManager.GetExtension("SafeMode");
        }

        /// <summary>
        /// Loads only installed themes
        /// </summary>
        public IEnumerable<ExtensionDescriptor> GetInstalledThemes() {
            return GetThemes(_extensionManager.AvailableExtensions());
        }

        private IEnumerable<ExtensionDescriptor> GetThemes(IEnumerable<ExtensionDescriptor> extensions) {
            var themes = new List<ExtensionDescriptor>();
            foreach (var descriptor in extensions) {

                if (!DefaultExtensionTypes.IsTheme(descriptor.ExtensionType)) {
                    continue;
                }

                ExtensionDescriptor theme = descriptor;

                if (theme.Tags == null || !theme.Tags.Contains("hidden")) {
                    themes.Add(theme);
                }
            }
            return themes;
        }

        /// <summary>
        /// Determines if a theme was recently installed by using the project's last written time.
        /// </summary>
        /// <param name="extensionDescriptor">The extension descriptor.</param>
        public bool IsRecentlyInstalled(ExtensionDescriptor extensionDescriptor) {
            DateTime lastWrittenUtc = _cacheManager.Get(extensionDescriptor, descriptor => {
                string projectFile = GetManifestPath(extensionDescriptor);
                if (!string.IsNullOrEmpty(projectFile)) {
                    // If project file was modified less than 24 hours ago, the module was recently deployed
                    return _virtualPathProvider.GetFileLastWriteTimeUtc(projectFile);
                }

                return DateTime.UtcNow;
            });

            return DateTime.UtcNow.Subtract(lastWrittenUtc) < new TimeSpan(1, 0, 0, 0);
        }

        private string GetManifestPath(ExtensionDescriptor descriptor) {
            string projectPath = _virtualPathProvider.Combine(descriptor.Location, descriptor.Id,
                                                       "theme.txt");

            if (!_virtualPathProvider.FileExists(projectPath)) {
                return null;
            }

            return projectPath;
        }

        private void GenerateWarning(string messageFormat, string featureName, IEnumerable<string> featuresInQuestion) {
            if (featuresInQuestion.Count() < 1)
                return;

            Services.Notifier.Warning(T(
                messageFormat,
                featureName,
                featuresInQuestion.Count() > 1
                    ? string.Join("",
                                  featuresInQuestion.Select(
                                      (fn, i) =>
                                      T(i == featuresInQuestion.Count() - 1
                                            ? "{0}"
                                            : (i == featuresInQuestion.Count() - 2
                                                   ? "{0} and "
                                                   : "{0}, "), fn).ToString()).ToArray())
                    : featuresInQuestion.First()));
        }

        public void DisablePreviewFeatures(IEnumerable<string> features) {
             foreach (var featureId in _featureManager.DisableFeatures(features,true)) {
                 var featureName = _featureManager.GetAvailableFeatures().First(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase)).Name;
                        Services.Notifier.Information(T("{0} was disabled", featureName));
             }
        }
    }
}
