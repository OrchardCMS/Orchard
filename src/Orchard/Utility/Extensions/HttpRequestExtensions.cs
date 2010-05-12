using System.Web;

namespace Orchard.Utility.Extensions {
    public static class HttpRequestExtensions {
        /// <summary>
        /// Returns the root part of a request. 
        /// </summary>
        /// <remarks>Prevents port number issues by using the client requested host</remarks>
        public static string ToRootUrlString(this HttpRequest request) {
            return string.Format("{0}://{1}", request.Url.Scheme, request.Headers["Host"]);
        }
    }
}