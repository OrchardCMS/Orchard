using System.Web.Routing;
using JetBrains.Annotations;

namespace Orchard.Themes.Services {
    [UsedImplicitly]
    public class SafeModeThemeSelector : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            return new ThemeSelectorResult {Priority = -100, ThemeName = "Themes"};
        }
    }
}