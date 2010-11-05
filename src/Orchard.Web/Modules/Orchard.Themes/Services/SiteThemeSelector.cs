using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    [UsedImplicitly]
    public class SiteThemeSelector : IThemeSelector {

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }

    public interface ISiteThemeService : IDependency {
        ExtensionDescriptor GetSiteTheme();
        void SetSiteTheme(string themeName);
    }

    public class SiteThemeService : ISiteThemeService {
        private readonly IExtensionManager _extensionManager;

        public SiteThemeService(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ExtensionDescriptor GetSiteTheme() {
            string currentThemeName = CurrentSite.As<ThemeSiteSettingsPart>().CurrentThemeName;

            if (string.IsNullOrEmpty(currentThemeName)) {
                return null;
            }
            return _extensionManager.GetExtensionDescriptor(currentThemeName);
        }

        public void SetSiteTheme(string themeName) {
            CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName = themeName;
        }
    }
}
