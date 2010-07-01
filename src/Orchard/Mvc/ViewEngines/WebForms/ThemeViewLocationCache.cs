using System;
using System.Web.Mvc;
using System.Web.Caching;
using System.Web;
using System.Web.Hosting;

namespace Orchard.Mvc.ViewEngines.WebForms {
    public class ThemeViewLocationCache : IViewLocationCache {
        private readonly string _requestTheme;

        public ThemeViewLocationCache(string requestTheme) {
            _requestTheme = requestTheme;
        }

        private string AlterKey(string key) {
            return key + ":" + _requestTheme;
        }

        public string GetViewLocation(HttpContextBase httpContext, string key) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }

            return (string)httpContext.Cache[AlterKey(key)];
        }

        public void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath) {
            if (httpContext == null) {
                throw new ArgumentNullException("httpContext");
            }

            httpContext.Cache.Insert(AlterKey(key), virtualPath, new CacheDependency(HostingEnvironment.MapPath("~/Themes")));
        }
    }
}
