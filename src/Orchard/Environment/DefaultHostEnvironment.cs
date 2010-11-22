using System.IO;
using System.Web;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Environment
{
    public class DefaultHostEnvironment : HostEnvironment
    {
        private readonly IClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultHostEnvironment(IClock clock, IHttpContextAccessor httpContextAccessor) {
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void ResetSiteCompilation()
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