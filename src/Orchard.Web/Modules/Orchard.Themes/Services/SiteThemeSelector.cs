using System;
using System.Web.Routing;
using Orchard.Localization;
namespace Orchard.Themes.Services {
    public class SiteThemeSelector : IThemeSelector {
        private readonly ISiteThemeService _siteThemeService;
        public Localizer T { get; set; }

        public SiteThemeSelector(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService; T = NullLocalizer.Instance;
        }
        public string GetTheme()
        {
            return _siteThemeService.GetCurrentThemeName();
        }
        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = _siteThemeService.GetCurrentThemeName();
            return String.IsNullOrEmpty(currentThemeName) ? null : new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
        public bool CanSet { get { return true; } }
        public void SetTheme(string themeName)
        {
            _siteThemeService.SetSiteTheme(themeName);
        }

        public string Tag { get { return "site"; } }

        public LocalizedString DisplayName { get { return T("SiteTheme"); } }

        public string Name { get { return "siteTheme"; } }
    }
}
