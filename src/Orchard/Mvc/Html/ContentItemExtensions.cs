using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContent content) {
            var display = content.As<IContentDisplayInfo>();
            if (display == null)
                return null;

            var values = display.DisplayRouteValues();
            return html.ActionLink(linkText ?? display.DisplayText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContent content) {
            return ItemDisplayLink(html, null, content);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content) {
            var display = content.As<IContentDisplayInfo>();
            if (display == null)
                return null;

            var values = display.EditRouteValues();
            return html.ActionLink(linkText ?? display.DisplayText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContent content) {
            return ItemEditLink(html, null, content);
        }
    }
}
