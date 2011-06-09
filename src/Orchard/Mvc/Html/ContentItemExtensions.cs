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

        public static MvcHtmlString ItemAdminLink(this HtmlHelper html, IContent content) {
            return ItemAdminLink(html, null, content);
        }

        public static MvcHtmlString ItemAdminLink(this HtmlHelper html, string linkText, IContent content) {
            return html.ItemAdminLink(linkText, content, null);
        }

        public static MvcHtmlString ItemAdminLink(this HtmlHelper html, string linkText, IContent content, object additionalRouteValues) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.AdminRouteValues == null)
                return null;

            return html.ActionLink(
                NonNullOrEmpty(linkText, metadata.DisplayText, content.ContentItem.TypeDefinition.DisplayName),
                Convert.ToString(metadata.AdminRouteValues["action"]),
                additionalRouteValues == null ? metadata.AdminRouteValues : MergeRouteValues(metadata.AdminRouteValues, new RouteValueDictionary(additionalRouteValues)));
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

        private static string NonNullOrEmpty(params string[] values) {
            foreach (var value in values) {
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return null;
        }

        private static RouteValueDictionary MergeRouteValues(RouteValueDictionary dictionary, RouteValueDictionary dictionaryToMerge) {
            if (dictionaryToMerge == null)
                return dictionary;

            var newDictionary = new RouteValueDictionary(dictionary);
            foreach (var valueDictionary in dictionaryToMerge)
                newDictionary[valueDictionary.Key] = valueDictionary.Value;

            return newDictionary;
        }
    }
}
