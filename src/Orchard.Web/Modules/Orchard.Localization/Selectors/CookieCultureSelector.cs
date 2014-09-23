using System;
using System.Web;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization.Providers;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Services;

namespace Orchard.Localization.Selectors {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CookieCultureSelector : ICultureSelector, ICultureStorageProvider {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClock _clock;
        private readonly ShellSettings _shellSettings;

        private const string CookieName = "OrchardCurrentCulture";
        private const int DefaultExpireTimeYear = 1;

        public CookieCultureSelector(IHttpContextAccessor httpContextAccessor,
            IClock clock,
            ShellSettings shellSettings) {
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
            _shellSettings = shellSettings;
        }

        public void SetCulture(string culture) {
            var httpContext = _httpContextAccessor.Current();

            if (httpContext == null) return;

            var cookie = new HttpCookie(CookieName, culture) {
                Expires = _clock.UtcNow.AddYears(DefaultExpireTimeYear), 
            };

            if (!String.IsNullOrEmpty(_shellSettings.RequestUrlHost)) {
                cookie.Domain = _shellSettings.RequestUrlHost;
            }

            if (!String.IsNullOrEmpty(_shellSettings.RequestUrlPrefix)) {
                cookie.Path = GetCookiePath(httpContext);
            }

            httpContext.Request.Cookies.Remove(CookieName);
            httpContext.Response.Cookies.Remove(CookieName);
            httpContext.Response.Cookies.Add(cookie);
        }

        public CultureSelectorResult GetCulture(HttpContextBase context) {
            if (context == null) return null;

            var cookie = context.Request.Cookies.Get(CookieName);

            if (cookie != null)
                return new CultureSelectorResult { Priority = -1, CultureName = cookie.Value };

            return null;
        }

        private string GetCookiePath(HttpContextBase httpContext) {
            var cookiePath = httpContext.Request.ApplicationPath;
            if (cookiePath != null && cookiePath.Length > 1) {
                cookiePath += '/';
            }

            cookiePath += _shellSettings.RequestUrlPrefix;

            return cookiePath;
        }
    }
}