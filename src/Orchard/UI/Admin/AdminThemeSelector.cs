using System.Web.Routing;
using Orchard.Themes;

namespace Orchard.UI.Admin {
    public class AdminThemeSelector : IThemeSelector {
        public ThemeSelectorResult GetTheme(RequestContext context) {
            if (IsApplied(context)) {
                return new ThemeSelectorResult { Priority = 100, ThemeName = "TheAdmin" };
            }

            return null;
        }

        public static void Apply(RequestContext context) {
            // the value isn't important
            context.HttpContext.Items[typeof(AdminThemeSelector)] = null;
        }

        public static bool IsApplied(RequestContext context) {
            return context.HttpContext.Items.Contains(typeof(AdminThemeSelector));
        }
    }
}
