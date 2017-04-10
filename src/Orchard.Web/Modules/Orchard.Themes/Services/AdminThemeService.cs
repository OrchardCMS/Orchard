using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    public interface IAdminThemeService : IDependency {
        ExtensionDescriptor GetAdminTheme();
        void SetTheme(string themeName);
        string GetCurrentThemeName();
    }

    public class AdminThemeService : IAdminThemeService
    {
        public const string CurrentThemeSignal = "AdminCurrentTheme";

        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IOrchardServices _orchardServices;

        public AdminThemeService(
            IOrchardServices orchardServices,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            ISignals signals) {

            _orchardServices = orchardServices;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _signals = signals;
        }

        public ExtensionDescriptor GetAdminTheme() {
            string currentThemeName = GetCurrentThemeName();
            return string.IsNullOrEmpty(currentThemeName) ? null : _extensionManager.GetExtension(GetCurrentThemeName());
        }

        public void SetTheme(string themeName) {
            var site = _orchardServices.WorkContext.CurrentSite;
            site.As<ThemeSiteSettingsPart>().CurrentAdminThemeName = themeName;

            _signals.Trigger(CurrentThemeSignal);
        }

        public string GetCurrentThemeName() {
            return _cacheManager.Get("CurrentAdminThemeName", ctx => {
                ctx.Monitor(_signals.When(CurrentThemeSignal));
                return _orchardServices.WorkContext.CurrentSite.As<ThemeSiteSettingsPart>().CurrentAdminThemeName;
            });
        }
    }
}
