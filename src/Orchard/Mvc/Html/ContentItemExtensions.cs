using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContentItemPart item) {
            return ItemDisplayLink(html, linkText, item.ContentItem);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContentItemPart item) {
            return ItemDisplayLink(html, item.ContentItem);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContentItemPart item) {
            return ItemEditLink(html, linkText, item.ContentItem);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContentItemPart item) {
            return ItemEditLink(html, item.ContentItem);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, ContentItem item) {
            var display = item.As<IContentItemDisplay>();
            if (display == null)
                return null;

            var values = display.DisplayRouteValues();
            return html.ActionLink(linkText ?? display.DisplayText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, ContentItem item) {
            return ItemDisplayLink(html, null, item);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, ContentItem item) {
            var display = item.As<IContentItemDisplay>();
            if (display == null)
                return null;

            var values = display.EditRouteValues();
            return html.ActionLink(linkText ?? display.DisplayText, Convert.ToString(values["action"]), values);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, ContentItem item) {
            return ItemEditLink(html, null, item);
        }
    }
}
