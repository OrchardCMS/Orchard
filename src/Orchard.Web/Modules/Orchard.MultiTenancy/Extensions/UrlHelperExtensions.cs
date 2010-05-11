using System.Web.Mvc;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Extensions {
    public static class UrlHelperExtensions {
        public static string Tenant(this UrlHelper urlHelper, ShellSettings tenantShellSettings) {
            //info: (heskew) might not keep the port insertion around beyond...
            var port = string.Empty;
            string host = urlHelper.RequestContext.HttpContext.Request.Headers["Host"];

            if(host.Contains(":"))
                port = host.Substring(host.IndexOf(":"));

             return string.Format(
               "http://{0}/{1}",
                 !string.IsNullOrEmpty(tenantShellSettings.RequestUrlHost)
                     ? tenantShellSettings.RequestUrlHost + port : host,
                tenantShellSettings.RequestUrlPrefix);
        }
    }
}