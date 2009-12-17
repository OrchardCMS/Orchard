using System.Web.Routing;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class SafeModeThemeSelector : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            return new ThemeSelectorResult {Priority = -100, ThemeName = "Themes"};
        }
    }
}