using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    [UsedImplicitly]
    public class SafeModeThemeSelector : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            return new ThemeSelectorResult {Priority = -100, ThemeName = "Themes"};
        }
    }
}