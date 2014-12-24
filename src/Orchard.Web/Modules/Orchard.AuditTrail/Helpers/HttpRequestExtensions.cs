using System;
using System.Web;
using Orchard.Utility.Extensions;

namespace Orchard.AuditTrail.Helpers {
    public static class HttpRequestExtensions {
        public static string GetReturnUrl(this HttpRequestBase request, string returnUrl = null, Func<string> defaultReturnUrl = null) {
            return request.IsLocalUrl(returnUrl)
                ? returnUrl
                : request.UrlReferrer != null
                    ? request.UrlReferrer.ToString()
                    : defaultReturnUrl != null
                        ? defaultReturnUrl()
                        : "~/Admin";
        }
    }
}