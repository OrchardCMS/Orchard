using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Html {
    public static class ContentItemExtensions {

        public static MvcHtmlString ItemDisplayText(this HtmlHelper html, IContent content) {
            return ItemDisplayText(html, content, true);
        }
        
        public static MvcHtmlString ItemDisplayText(this HtmlHelper html, IContent content, bool encode) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayText == null)
                return null;
            if (encode) {
                return MvcHtmlString.Create(html.Encode(metadata.DisplayText));
            } else {
                return MvcHtmlString.Create(metadata.DisplayText);
            }
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContent content) {
            return ItemDisplayLink(html, null, content, null);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, IContent content, object htmlAttributes) {
            return ItemDisplayLink(html, null, content, htmlAttributes);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContent content) {
            return ItemDisplayLink(html, linkText, content, null);
        }

        public static MvcHtmlString ItemDisplayLink(this HtmlHelper html, string linkText, IContent content, object htmlAttributes = null) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return html.ActionLink(
                NonNullOrEmpty(linkText, metadata.DisplayText, "view"),
                Convert.ToString(metadata.DisplayRouteValues["action"]),
                metadata.DisplayRouteValues,
                new RouteValueDictionary(htmlAttributes));
        }

        public static string ItemDisplayUrl(this UrlHelper urlHelper, IContent content) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.DisplayRouteValues == null)
                return null;

            return urlHelper.Action(
                Convert.ToString(metadata.DisplayRouteValues["action"]),
                metadata.DisplayRouteValues);
        }

        public static MvcHtmlString ItemRemoveLink(this HtmlHelper html, IContent content) {
            return ItemRemoveLink(html, null, content, null);
        }

        public static MvcHtmlString ItemRemoveLink(this HtmlHelper html, string linkText, IContent content, object additionalRouteValues) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.RemoveRouteValues == null)
                return null;

            return html.ActionLink(
                NonNullOrEmpty(linkText, metadata.DisplayText, "remove"),
                Convert.ToString(metadata.RemoveRouteValues["action"]),
                metadata.RemoveRouteValues.Merge(additionalRouteValues));
        }

        public static string ItemRemoveUrl(this UrlHelper urlHelper, IContent content, object additionalRouteValues) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.RemoveRouteValues == null)
                return null;

            return urlHelper.Action(
                Convert.ToString(metadata.RemoveRouteValues["action"]),
                metadata.RemoveRouteValues.Merge(additionalRouteValues));
        }

        public static MvcHtmlString ItemEditLinkWithReturnUrl(this HtmlHelper html, string linkText, IContent content) {
            return html.ItemEditLink(linkText, content, new { ReturnUrl = html.ViewContext.HttpContext.Request.RawUrl });
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content) {
            return html.ItemEditLink(linkText, content, null);
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content, object additionalRouteValues) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.EditorRouteValues == null)
                return null;

            return html.ActionLink(
                NonNullOrEmpty(linkText, metadata.DisplayText, content.ContentItem.TypeDefinition.DisplayName),
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues.Merge(additionalRouteValues));
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, string linkText, IContent content, object additionalRouteValues, object htmlAttributes) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.EditorRouteValues == null)
                return null;

            return html.ActionLink(
                NonNullOrEmpty(linkText, metadata.DisplayText, content.ContentItem.TypeDefinition.DisplayName),
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues.Merge(additionalRouteValues),
                htmlAttributes);
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
                metadata.AdminRouteValues.Merge(additionalRouteValues));
        }

        public static string ItemEditUrl(this UrlHelper urlHelper, IContent content, object additionalRouteValues = null) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            if (metadata.EditorRouteValues == null)
                return null;

            return urlHelper.Action(
                Convert.ToString(metadata.EditorRouteValues["action"]),
                metadata.EditorRouteValues.Merge(additionalRouteValues ?? new {}));
        }

        public static string ItemAdminUrl(this UrlHelper urlHelper, IContent content, object additionalRouteValues = null) {
            var metadata = content.ContentItem.ContentManager.GetItemMetadata(content);
            return metadata.AdminRouteValues == null ? null : urlHelper.RouteUrl(metadata.AdminRouteValues.Merge(additionalRouteValues ?? new { }));
        }

        private static string NonNullOrEmpty(params string[] values) {
            foreach (var value in values) {
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return null;
        }

        public static MvcHtmlString ItemEditLink(this HtmlHelper html, IContent content) {
            return ItemEditLink(html, null, content);
        }
    }
}
