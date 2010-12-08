using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    public interface ISiteThemeService : IDependency {
        ExtensionDescriptor GetSiteTheme();
        void SetSiteTheme(string themeName);
    }

    public class SiteThemeService : ISiteThemeService {
        private readonly IExtensionManager _extensionManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public SiteThemeService(IExtensionManager extensionManager, IWorkContextAccessor workContextAccessor) {
            _extensionManager = extensionManager;
            _workContextAccessor = workContextAccessor;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionDescriptor GetSiteTheme() {
            var site = _workContextAccessor.GetContext().CurrentSite;
            string currentThemeName = site.As<ThemeSiteSettingsPart>().CurrentThemeName;

            return string.IsNullOrEmpty(currentThemeName) ? null : _extensionManager.GetExtension(currentThemeName);
        }

        public void SetSiteTheme(string themeName) {
            var site = _workContextAccessor.GetContext().CurrentSite;
            site.As<ThemeSiteSettingsPart>().Record.CurrentThemeName = themeName;
        }
    }
}