using System.Web.Routing;
using Orchard.Localization;
using Orchard.UI.Admin;
namespace Orchard.Themes.Services
{
    public class AdminThemeSelector : IThemeSelector {
        private readonly IAdminThemeService _adminThemeService;
        public AdminThemeSelector(IAdminThemeService adminThemeService)
        {
            _adminThemeService = adminThemeService;
            T = NullLocalizer.Instance;
        }
        public ThemeSelectorResult GetTheme(RequestContext context) {
            if (AdminFilter.IsApplied(context)) {
                string currentThemeName = _adminThemeService.GetCurrentThemeName();
                return string.IsNullOrEmpty(currentThemeName) ? new ThemeSelectorResult { Priority = 100, ThemeName = "TheAdmin" } : new ThemeSelectorResult { Priority = 100, ThemeName = currentThemeName };

                
            }

            return null;
        }
        public Localizer T { get; set; }
        public bool CanSet { get { return true; } }
        public string GetTheme()
        {
            return _adminThemeService.GetCurrentThemeName();
        }
        public void SetTheme(string themeName)
        {
            _adminThemeService.SetTheme(themeName);
        }

        public string Tag { get { return "admin"; } }

        public LocalizedString DisplayName { get { return T("AdminTheme"); } }

        public string Name { get { return "adminTheme"; } }
    }
}
