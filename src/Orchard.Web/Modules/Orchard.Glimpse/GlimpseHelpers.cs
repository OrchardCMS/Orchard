using System;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Glimpse {
    public static class GlimpseHelpers {
        public static string AppendReturnUrl(string path, UrlHelper urlHelper) {
            var requestUrl = urlHelper.RequestContext.HttpContext.Request.Url;

            if (requestUrl == null) {
                return path;
            }

            var uriBuilder = new UriBuilder(requestUrl.AbsoluteUri) { Path = path };
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            query["returnUrl"] = requestUrl.AbsolutePath;
            uriBuilder.Query = query.ToString();
            path = uriBuilder.ToString();
            
            return path;
        }
    }
}