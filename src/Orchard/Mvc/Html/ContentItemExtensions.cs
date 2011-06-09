using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {
        public static MvcHtmlString ItemDisplayText(this HtmlHelper html, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayText == null)
                return null;
            return MvcHtmlString.Create(html.Encode(metadata.DisplayText));
        }

        public static string ItemDisplayUrl(this UrlHelper urlHelper, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return urlHelper.Action(
                Convert.ToString(metadata.DisplayRouteValues["action"]),
                metadata.DisplayRouteValues);
        }

        public static string ItemEditUrl(this UrlHelper urlHelper, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return urlHelper.Action(
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues);
        }
    }
}
