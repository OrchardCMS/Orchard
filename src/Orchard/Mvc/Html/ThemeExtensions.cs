using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI;
using Orchard.Environment.Extensions;
using Orchard.Themes;
using Orchard.Validation;

namespace Orchard.Mvc.Html {
    public static class ThemeExtensions {
        /// <summary>
        /// Include a view from the current view.
        /// </summary>
        public static void Include(this HtmlHelper helper, string viewName) {
            Argument.ThrowIfNull(helper, "helper");
            helper.RenderPartial(viewName);
        }

        public static string ThemePath(this HtmlHelper helper, string path) {
            return helper.ThemePath(helper.Resolve<IThemeService>().GetRequestTheme(helper.ViewContext.RequestContext), path);
        }

        public static string ThemePath(this HtmlHelper helper, ITheme theme, string path) {
            return helper.Resolve<IExtensionManager>().GetThemeLocation(theme) + path;
        }
    }
}
