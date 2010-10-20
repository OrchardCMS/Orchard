using System;
using System.Web;
using System.Web.Mvc;

namespace Orchard.Utility.Extensions {
    public static class HttpRequestExtensions {
        /// <summary>
        /// Returns the root part of a request. 
        /// </summary>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToRootUrlString(this HttpRequestBase request) {
            return string.Format("{0}://{1}", request.Url.Scheme, request.Headers["Host"]);
        }

        /// <summary>
        /// Returns the root part of a request. 
        /// </summary>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToRootUrlString(this HttpRequest request) {
            return string.Format("{0}://{1}", request.Url.Scheme, request.Headers["Host"]);
        }

        /// <summary>
        /// Returns the client requested url. 
        /// </summary>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToUrlString(this HttpRequestBase request) {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        /// <summary>
        /// Returns the client requested url. 
        /// </summary>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToUrlString(this HttpRequest request) {
            return string.Format("{0}://{1}{2}", request.Url.Scheme, request.Headers["Host"], request.RawUrl);
        }

        public static string AbsoluteAction(this UrlHelper url, string action, string controller, object routeValues) {
            Uri requestUrl = url.RequestContext.HttpContext.Request.Url;
            return string.Concat(requestUrl.GetLeftPart(UriPartial.Authority),url.Action(action, controller, routeValues));
        }

    }
}
