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

        public static string MakeAbsolute(this UrlHelper urlHelper, string url, string baseUrl = null) {
            if(url != null && url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
                return url;
            }
            
            if(String.IsNullOrEmpty(baseUrl)) {
                var workContextAccessor = urlHelper.RequestContext.GetWorkContext();
                baseUrl = workContextAccessor.CurrentSite.BaseUrl;

                if (String.IsNullOrWhiteSpace(baseUrl)) {
                    baseUrl = urlHelper.RequestContext.HttpContext.Request.ToApplicationRootUrlString();
                }
            }

            if(String.IsNullOrEmpty(url)) {
                return baseUrl;
            }

            // remove any application path from the base url
            var applicationPath = urlHelper.RequestContext.HttpContext.Request.ApplicationPath ?? String.Empty;

            // ~/foo/bar => /foo/bar
            if (url.StartsWith("~/", StringComparison.OrdinalIgnoreCase)) {
                url = url.Substring(1);
            }

            // orchardlocal/foo/bar => /orchardlocal/foo/bar
            if(!url.StartsWith("/")) {
                url = "/" + url;
            }
            
            // /orchardlocal/foo/bar => foo/bar
            if (url.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase)) {
                url = url.Substring(applicationPath.Length);
            }

            baseUrl = baseUrl.TrimEnd('/');
            url = url.TrimStart('/');
            
            return baseUrl + "/" + url;
        }
    }
}
