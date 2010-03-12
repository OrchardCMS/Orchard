using System.Web.Routing;
using Orchard.Themes;

namespace Orchard.UI.Admin {
    public class AdminThemeSelector : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            if (AdminFilter.IsApplied(context)) {
                return new ThemeSelectorResult { Priority = 100, ThemeName = "TheAdmin" };
            }

            return null;
        }

    }
}
