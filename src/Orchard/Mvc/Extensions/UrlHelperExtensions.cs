using System;
using System.Web.Mvc;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Extensions {
    public static class UrlHelperExtensions {
        public static string AbsoluteAction(this UrlHelper urlHelper, Func<string> urlAction) {
            return urlHelper.MakeAbsolute(urlAction());
        }

        private static string MakeAbsolute(this UrlHelper urlHelper, string url) {
            var siteUrl = urlHelper.RequestContext.HttpContext.Request.ToRootUrlString();
            return siteUrl + url;
        }
    }
}
