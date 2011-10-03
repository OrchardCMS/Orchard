using System;
using System.Web.Mvc;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Extensions {
    public static class UrlHelperExtensions {
        public static string AbsoluteAction(this UrlHelper urlHelper, Func<string> urlAction) {
            return urlHelper.MakeAbsolute(urlAction());
        }

        public static string AbsoluteAction(this UrlHelper urlHelper, string actionName) {
            return urlHelper.MakeAbsolute(urlHelper.Action(actionName));
        }

        public static string AbsoluteAction(this UrlHelper urlHelper, string actionName, object routeValues) {
            return urlHelper.MakeAbsolute(urlHelper.Action(actionName, routeValues));
        }

        public static string AbsoluteAction(this UrlHelper urlHelper, string actionName, string controller) {
            return urlHelper.MakeAbsolute(urlHelper.Action(actionName, controller));
        }

        public static string AbsoluteAction(this UrlHelper urlHelper, string actionName, string controller, object routeValues) {
            return urlHelper.MakeAbsolute(urlHelper.Action(actionName, controller, routeValues));
        }

        public static string MakeAbsolute(this UrlHelper urlHelper, string url) {
            var siteUrl = urlHelper.RequestContext.HttpContext.Request.ToRootUrlString();
            return siteUrl + url;
        }
    }
}
