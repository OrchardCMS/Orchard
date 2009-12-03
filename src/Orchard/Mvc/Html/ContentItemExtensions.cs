using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.Models;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static string ItemDisplayText(this HtmlHelper html, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayText == null)
                return null;
            return html.Encode(metadata.DisplayText);
        }


        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return html.ActionLink(
                linkText ?? metadata.DisplayText,
                Convert.ToString(metadata.DisplayRouteValues["action"]),
                metadata.DisplayRouteValues);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContent content) {
            return ItemDisplayLink(html, null, content);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.EditorRouteValues == null)
                return null;

            return html.ActionLink(
                linkText ?? metadata.DisplayText,
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContent content) {
            return ItemEditLink(html, null, content);
        }
    }
}
