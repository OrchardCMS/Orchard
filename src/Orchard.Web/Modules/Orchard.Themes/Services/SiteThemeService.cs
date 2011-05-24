using Orchard.Caching;
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
        string GetCurrentThemeName();
    }

    public class SiteThemeService : ISiteThemeService {
        public const string CurrentThemeSignal = "SiteCurrentTheme";

        private readonly IExtensionManager _extensionManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IOrchardServices _orchardServices;

        public SiteThemeService(
            IOrchardServices orchardServices,
            IExtensionManager extensionManager,
            IWorkContextAccessor workContextAccessor,
            ICacheManager cacheManager,
            ISignals signals) {

            _orchardServices = orchardServices;
            _extensionManager = extensionManager;
            _workContextAccessor = workContextAccessor;
            _cacheManager = cacheManager;
            _signals = signals;

            Logger = NullLogger.Instance;

            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionDescriptor GetSiteTheme() {
            string currentThemeName = GetCurrentThemeName();
            return string.IsNullOrEmpty(currentThemeName) ? null : _extensionManager.GetExtension(GetCurrentThemeName());
        }

        public void SetSiteTheme(string themeName) {
            var site = _workContextAccessor.GetContext().CurrentSite;
            site.As<ThemeSiteSettingsPart>().CurrentThemeName = themeName;

            _signals.Trigger(CurrentThemeSignal);
        }

        public string GetCurrentThemeName() {
            return _cacheManager.Get("CurrentThemeName", ctx => {
                ctx.Monitor(_signals.When(CurrentThemeSignal));
                return _orchardServices.WorkContext.CurrentSite.As<ThemeSiteSettingsPart>().CurrentThemeName;
            });
        }
    }
}
