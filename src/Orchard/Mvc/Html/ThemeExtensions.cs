using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Environment.Extensions.Models;
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

        [Obsolete("How do you know the request theme is the same as the place the theme template is rendering from?")]
        public static string ThemePath(this HtmlHelper helper, string path) {
            return helper.ThemePath(helper.Resolve<IThemeManager>().GetRequestTheme(helper.ViewContext.RequestContext), path);
        }

        public static string ThemePath(this HtmlHelper helper, ExtensionDescriptor theme, string path) {
            return theme.Location + "/" + theme.Id + path;
        }
    }
}
