using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Environment
{
    public class DefaultHostEnvironment : IHostEnvironment
    {
        private readonly IClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultHostEnvironment(IClock clock, IHttpContextAccessor httpContextAccessor) {
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsFullTrust
        {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public string MapPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public bool IsAssemblyLoaded(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => new AssemblyName(assembly.FullName).Name == name);
        }

        public void RestartAppDomain()
        {
            ResetSiteCompilation();
        }

        public void ResetSiteCompilation()
        {
            // Touch web.config
            File.SetLastWriteTimeUtc(MapPath("~/web.config"), _clock.UtcNow);

            // If setting up extensions/modules requires an AppDomain restart, it's very unlikely the
            // current request can be processed correctly.  So, we redirect to the same URL, so that the
            // new request will come to the newly started AppDomain.
            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null)
            {
                // Don't redirect posts...
                if (httpContext.Request.RequestType == "GET")
                {
                    httpContext.Response.Redirect(HttpContext.Current.Request.ToUrlString(), true /*endResponse*/);
                }
                else
                {
                    httpContext.Response.WriteFile("~/Refresh.html");
                    httpContext.Response.End();
                }
            }
        }
    }
}