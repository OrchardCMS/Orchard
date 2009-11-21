using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContentItemPart item) {
            var display = item.As<IContentItemDisplay>();
            var values = display.DisplayRouteValues();
            return html.ActionLink(linkText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContentItemPart item) {
            var display = item.As<IContentItemDisplay>();
            var values = display.DisplayRouteValues();
            return html.ActionLink(display.DisplayText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContentItemPart item) {
            var display = item.As<IContentItemDisplay>();
            var values = display.DisplayRouteValues();
            return html.ActionLink(linkText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContentItemPart item) {
            var display = item.As<IContentItemDisplay>();
            var values = display.DisplayRouteValues();
            return html.ActionLink(display.DisplayText, Convert.ToString(values["action"]), values);
        }
    }
}
