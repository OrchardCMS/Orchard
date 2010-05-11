using System.Web.Mvc;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Extensions {
    public static class UrlHelperExtensions {
        public static string Tenant(this UrlHelper urlHelper, ShellSettings tenantShellSettings) {
            //info: (heskew) might not keep the port insertion around beyond...
            var port = urlHelper.RequestContext.HttpContext.Request.Url.Port;
            return string.Format(
                "http://{0}{2}/{1}",
                !string.IsNullOrEmpty(tenantShellSettings.RequestUrlHost)
                    ? tenantShellSettings.RequestUrlHost
                    : urlHelper.RequestContext.HttpContext.Request.Url.Host,
                tenantShellSettings.RequestUrlPrefix,
                port != 80 ? string.Format(":{0}", port) : "");
        }
    }
}