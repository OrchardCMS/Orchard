using System.Web.Mvc;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Extensions {
    public static class UrlHelperExtensions {
        public static string Tenant(this UrlHelper urlHelper, ShellSettings tenantShellSettings) {
            return string.Format(
                "http://{0}/{1}",
                !string.IsNullOrEmpty(tenantShellSettings.RequestUrlHost)
                    ? tenantShellSettings.RequestUrlHost
                    : urlHelper.RequestContext.HttpContext.Request.Url.Host,
                tenantShellSettings.RequestUrlPrefix);
        }
    }
}