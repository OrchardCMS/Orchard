using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string Tenant(this UrlHelper urlHelper, ShellSettings tenantShellSettings)
        {

            var requestUrlHost = !string.IsNullOrEmpty(tenantShellSettings.RequestUrlHost) ? tenantShellSettings.RequestUrlHost.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).First() : null;

            //info: (heskew) might not keep the port/vdir insertion around beyond...
            var port = string.Empty;
            var host = urlHelper.RequestContext.HttpContext.Request.Headers["Host"];

            if (host.Contains(":"))
                port = host.Substring(host.IndexOf(":"));

            var result = string.Format("{0}://{1}",
                urlHelper.RequestContext.HttpContext.Request.Url.Scheme,
                !string.IsNullOrEmpty(requestUrlHost) ? requestUrlHost + port : host);

            var applicationPath = urlHelper.RequestContext.HttpContext.Request.ApplicationPath;
            if (!string.IsNullOrEmpty(applicationPath) && applicationPath != "/")
                result += applicationPath;

            if (!string.IsNullOrEmpty(tenantShellSettings.RequestUrlPrefix))
                result += "/" + tenantShellSettings.RequestUrlPrefix;

            return result;
        }
    }
}