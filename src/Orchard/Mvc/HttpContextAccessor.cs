using System;
using System.Collections.Concurrent;
using System.Web;
using Autofac;
using Orchard.Mvc.Extensions;

namespace Orchard.Mvc {
    public class HttpContextAccessor : IHttpContextAccessor {
        readonly object _contextKey = new object();

        [ThreadStatic]
        static ConcurrentDictionary<object, HttpContextBase> _threadStaticContexts;

        public HttpContextBase Current() {
            if (!HttpContext.Current.IsBackgroundContext())
                return new HttpContextWrapper(HttpContext.Current);

            return GetContext();
        }

        public HttpContextBase CreateContext(ILifetimeScope lifetimeScope) {
            return new MvcModule.HttpContextPlaceholder(
                _threadStaticContexts, 
                _contextKey, 
                () => "http://localhost" // Use a valid URL always for the fake request. The value itself doesn't matter.
            );
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