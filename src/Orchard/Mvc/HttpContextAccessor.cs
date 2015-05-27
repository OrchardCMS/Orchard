using System;
using System.Web;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        private HttpContextBase _httpContext;

        public HttpContextBase Current() {
            var httpContext = GetStaticProperty();
            return !IsBackgroundHttpContext(httpContext) ? new HttpContextWrapper(httpContext) : _httpContext;
        }

        public void Set(HttpContextBase httpContext) {
            _httpContext = httpContext;
        }

        private static bool IsBackgroundHttpContext(HttpContext httpContext) {
            return httpContext == null || httpContext.Items.Contains(BackgroundHttpContextFactory.IsBackgroundHttpContextKey);
        }

        [ThreadStatic]
        static ConcurrentDictionary<object, HttpContextBase> _threadStaticContexts;

        public HttpContextBase Current() {
            if (!HttpContext.Current.IsBackgroundContext())
                return new HttpContextWrapper(HttpContext.Current);

            return GetContext();
        }

        public HttpContextBase CreateContext(ILifetimeScope lifetimeScope) {
            return new MvcModule.HttpContextPlaceholder(_threadStaticContexts, _contextKey, () => {
                return lifetimeScope.Resolve<ISiteService>().GetSiteSettings().BaseUrl;
            });
        }

        private HttpContextBase GetContext() {
            HttpContextBase context;
            return ThreadStaticContexts.TryGetValue(_contextKey, out context) ? context : null;
        }
            
        static ConcurrentDictionary<object, HttpContextBase> ThreadStaticContexts {
            get {
                return _threadStaticContexts ?? (_threadStaticContexts = new ConcurrentDictionary<object, HttpContextBase>());
            }
        }
    }
}