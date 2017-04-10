using System.Web.Routing;
using Orchard.Localization;
namespace Orchard.Themes.Services {
    public class SafeModeThemeSelector : IThemeSelector {
        public SafeModeThemeSelector()
        {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public ThemeSelectorResult GetTheme(RequestContext context) {
            return new ThemeSelectorResult {Priority = -100, ThemeName = "SafeMode" };
        }
        public bool CanSet { get { return false; } }
        public void SetTheme(string themeName)
        { }
        public string GetTheme()
        {
            return "SafeMode";
        }
        public string Tag { get { return string.Empty; } }

        public LocalizedString DisplayName { get { return T("SafeMode"); } }

        public string Name { get { return "safeMode"; } }
    }
}