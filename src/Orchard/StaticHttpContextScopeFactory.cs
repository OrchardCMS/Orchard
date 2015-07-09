using System;
using System.IO;
using System.Web;
using Orchard.Settings;

namespace Orchard {
    public class StaticHttpContextScopeFactory : IStaticHttpContextScopeFactory {
        private readonly Func<ISiteService> _siteService;
        public StaticHttpContextScopeFactory(Func<ISiteService> siteService) {
            _siteService = siteService;
        }

        public const string IsBackgroundHttpContextKey = "IsBackgroundHttpContext";

        public IDisposable CreateStaticScope() {
            // If there already is a current HttpContext, use that one as the stub.
            if(HttpContext.Current != null)
                return new StaticHttpContextScope(HttpContext.Current);

            // We're in a background task (or some other static context like the console),
            // so create a stub context so that Html Helpers can still be executed when rendering shapes in background tasks
            // (sadly enought some Html Helpers access HttpContext.Current directly).
            var url = _siteService().GetSiteSettings().BaseUrl;
            var stub = new HttpContext(new HttpRequest("", url, ""), new HttpResponse(new StringWriter()));

            stub.Items[IsBackgroundHttpContextKey] = true;

            return new StaticHttpContextScope(stub);
        }
    }
}