using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Orchard.ContentManagement;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static MvcHtmlString ItemDisplayText(this HtmlHelper html, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayText == null)
                return null;
            return MvcHtmlString.Create(html.Encode(metadata.DisplayText));
        }


        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return html.ActionLink(
                linkText ?? metadata.DisplayText ?? "view",
                Convert.ToString(metadata.DisplayRouteValues["action"]),
                metadata.DisplayRouteValues);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContent content) {
            return ItemDisplayLink(html, null, content);
        }

        public static MvcHtmlString ItemEditLinkWithReturnUrl(this HtmlHelper html, string linkText, IContent content) {
            return html.ItemEditLink(linkText, content, new {ReturnUrl = html.ViewContext.HttpContext.Request.RawUrl});
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content) {
            return html.ItemEditLink(linkText, content, null);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content, object additionalRouteValues) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.EditorRouteValues == null)
                return null;

            return html.ActionLink(
                linkText ?? metadata.DisplayText ?? "edit",
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues.Merge(additionalRouteValues));
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContent content) {
            return ItemEditLink(html, null, content);
        }
    }
}
