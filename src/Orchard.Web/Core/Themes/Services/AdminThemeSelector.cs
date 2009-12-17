using System.Globalization;
using System.Web.Routing;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class AdminThemeSelector : IThemeSelector {
        
        public ThemeSelectorResult GetTheme(RequestContext context) {
            if (!context.HttpContext.Request.Path.StartsWith("/admin", true, CultureInfo.InvariantCulture))
                return null;

            return new ThemeSelectorResult { Priority = 0, ThemeName = "TheAdmin" };
        }
    }
}
