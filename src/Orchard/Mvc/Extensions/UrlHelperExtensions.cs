using System;
using System.Web.Mvc;

namespace Orchard.Mvc.Extensions {
    public static class UrlHelperExtensions {
        public static string AbsoluteAction(this UrlHelper urlHelper, Func<string> urlAction) {
            return urlHelper.MakeAbsolute(urlAction());
        }

        private static string MakeAbsolute(this UrlHelper urlHelper, string url) {
            var siteUrl = urlHelper.RequestContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            return siteUrl + url;
        }
    }
}
