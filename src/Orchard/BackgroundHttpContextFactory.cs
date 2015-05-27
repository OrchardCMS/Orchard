using System.IO;
using System.Web;
using Orchard.Settings;

namespace Orchard {
    /// <summary>
    /// A factory class that creates an HttpContext instance and initializes the HttpContext.Current property with that instance.
    /// This is useful when rendering views from a background thread, as some Html Helpers access HttpContext.Current directly, thus preventing a NullReferenceException.
    /// </summary>
    public class BackgroundHttpContextFactory : IBackgroundHttpContextFactory {
        public const string IsBackgroundHttpContextKey = "IsBackgroundHttpContext";
        private readonly ISiteService _siteService;
        public BackgroundHttpContextFactory(ISiteService siteService) {
            _siteService = siteService;
        }

        public HttpContext CreateHttpContext() {
            var url = _siteService.GetSiteSettings().BaseUrl;
            var httpContext = new HttpContext(new HttpRequest("", url, ""), new HttpResponse(new StringWriter()));

            httpContext.Items[IsBackgroundHttpContextKey] = true;

            return httpContext;
        }

        public void InitializeHttpContext() {
            if (HttpContext.Current != null)
                return;

            HttpContext.Current = CreateHttpContext();
        }
    }
}