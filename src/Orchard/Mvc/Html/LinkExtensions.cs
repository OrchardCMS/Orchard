using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace Orchard.Mvc.Html {
    public static class LinkExtensions {
        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName) {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, object routeValues) {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, ObjectToDictionary(routeValues), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, object routeValues, object htmlAttributes) {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, RouteValueDictionary routeValues) {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return ActionLink(htmlHelper, linkText, actionName, null /* controllerName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, string controllerName) {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, new RouteValueDictionary(), new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, string controllerName, object routeValues, object htmlAttributes) {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText.ToString())) {
                throw new ArgumentException("Argument must be a non empty string", "linkText");
            }
            return MvcHtmlString.Create(GenerateLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, null /* routeName */, actionName, controllerName, routeValues, htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) {
            return ActionLink(htmlHelper, linkText, actionName, controllerName, protocol, hostName, fragment, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString ActionLink(this HtmlHelper htmlHelper, IHtmlString linkText, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText.ToString())) {
                throw new ArgumentException("Argument must be a non empty string", "linkText");
            }
            return MvcHtmlString.Create(GenerateLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, null /* routeName */, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, object routeValues) {
            return RouteLink(htmlHelper, linkText, ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, RouteValueDictionary routeValues) {
            return RouteLink(htmlHelper, linkText, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName) {
            return RouteLink(htmlHelper, linkText, routeName, (object)null /* routeValues */);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, object routeValues) {
            return RouteLink(htmlHelper, linkText, routeName, ObjectToDictionary(routeValues));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, RouteValueDictionary routeValues) {
            return RouteLink(htmlHelper, linkText, routeName, routeValues, new RouteValueDictionary());
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, object routeValues, object htmlAttributes) {
            return RouteLink(htmlHelper, linkText, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return RouteLink(htmlHelper, linkText, null /* routeName */, routeValues, htmlAttributes);
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, object routeValues, object htmlAttributes) {
            return RouteLink(htmlHelper, linkText, routeName, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText.ToString())) {
                throw new ArgumentException("Argument must be a non empty string", "linkText");
            }
            return MvcHtmlString.Create(GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, routeName, routeValues, htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) {
            return RouteLink(htmlHelper, linkText, routeName, protocol, hostName, fragment, ObjectToDictionary(routeValues), HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcHtmlString RouteLink(this HtmlHelper htmlHelper, IHtmlString linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(linkText.ToString())) {
                throw new ArgumentException("Argument must be a non empty string", "linkText");
            }
            return MvcHtmlString.Create(GenerateRouteLink(htmlHelper.ViewContext.RequestContext, htmlHelper.RouteCollection, linkText, routeName, protocol, hostName, fragment, routeValues, htmlAttributes));
        }

        public static string GenerateLink(RequestContext requestContext, RouteCollection routeCollection, IHtmlString linkText, string routeName, string actionName, string controllerName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLink(requestContext, routeCollection, linkText, routeName, actionName, controllerName, null /* protocol */, null /* hostName */, null /* fragment */, routeValues, htmlAttributes);
        }

        public static string GenerateLink(RequestContext requestContext, RouteCollection routeCollection, IHtmlString linkText, string routeName, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLinkInternal(requestContext, routeCollection, linkText, routeName, actionName, controllerName, protocol, hostName, fragment, routeValues, htmlAttributes, true /* includeImplicitMvcValues */);
        }
        
        public static string GenerateRouteLink(RequestContext requestContext, RouteCollection routeCollection, IHtmlString linkText, string routeName, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateRouteLink(requestContext, routeCollection, linkText, routeName, null /* protocol */, null /* hostName */, null /* fragment */, routeValues, htmlAttributes);
        }

        public static string GenerateRouteLink(RequestContext requestContext, RouteCollection routeCollection, IHtmlString linkText, string routeName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes) {
            return GenerateLinkInternal(requestContext, routeCollection, linkText, routeName, null /* actionName */, null /* controllerName */, protocol, hostName, fragment, routeValues, htmlAttributes, false /* includeImplicitMvcValues */);
        }
        private static string GenerateLinkInternal(RequestContext requestContext, RouteCollection routeCollection, IHtmlString linkText, string routeName, string actionName, string controllerName, string protocol, string hostName, string fragment, RouteValueDictionary routeValues, IDictionary<string, object> htmlAttributes, bool includeImplicitMvcValues) {
            string url = UrlHelper.GenerateUrl(routeName, actionName, controllerName, protocol, hostName, fragment, routeValues, routeCollection, requestContext, includeImplicitMvcValues);
            TagBuilder tagBuilder = new TagBuilder("a") {
                InnerHtml = linkText.ToString()
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", url);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        private static RouteValueDictionary ObjectToDictionary(object value) {
            RouteValueDictionary dictionary = new RouteValueDictionary();

            if (value != null) {
                foreach (var prop in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                    dictionary.Add(prop.Name, prop.GetValue(value));
                }
            }

            return dictionary;
        }
    }
}
