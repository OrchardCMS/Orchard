using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Themes.Services;
using Orchard.UI.Notify;

namespace Orchard.Themes.Events {
    public class ThemeDisableEventHandler : IFeatureEventHandler {
        private readonly IFeatureManager _featureManager;
        private readonly ISiteThemeService _siteThemeService;
        private readonly INotifier _notifier;

        public ThemeDisableEventHandler(
            IFeatureManager featureManager,
            ISiteThemeService siteThemeService,
            INotifier notifier) {
            _featureManager = featureManager;
            _siteThemeService = siteThemeService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Installing(Feature feature) {
        }

        public void Installed(Feature feature) {
        }

        public void Enabling(Feature feature) {
        }

        public void Enabled(Feature feature) {
        }

        public void Disabling(Feature feature) {
        }

        public void Disabled(Feature feature) {
            var currentTheme = _siteThemeService.GetCurrentThemeName();
            if (feature.Descriptor.Name == currentTheme) {
                _siteThemeService.SetSiteTheme(null);

                // Notifications don't work in feature events. See: https://github.com/OrchardCMS/Orchard/issues/6106
                _notifier.Warning(T("The current theme was disabled, because one of its dependencies was disabled."));
            }
        }

        public void Uninstalling(Feature feature) {
        }

        public void Uninstalled(Feature feature) {
        }
    }
}