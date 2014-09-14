using System;
using System.Globalization;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Mvc;

namespace Orchard.Localization.Providers {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class BrowserCultureProvider : ICultureProvider {
        private readonly Lazy<ICultureManager> _cultureManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BrowserCultureProvider(
            Lazy<ICultureManager> cultureManager,
            IHttpContextAccessor httpContextAccessor) {
            _cultureManager = cultureManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCulture() {
            var httpContext = _httpContextAccessor.Current();

            if (httpContext == null) return null;

            /* Fall back to Browser */
            var userLanguages = httpContext.Request.UserLanguages;

            if (userLanguages == null || userLanguages.Length == 0)
                return null;

            var cultures = _cultureManager.Value.ListCultures().ToList();

            foreach (var userLanguage in userLanguages) {
                var language = userLanguage.Split(';')[0].Trim();

                if (cultures.Contains(language, StringComparer.OrdinalIgnoreCase)) {
                    try {
                        return CultureInfo.CreateSpecificCulture(language).Name;
                    }
                    catch (ArgumentException) {
                    }
                }
            }

            return null;
        }

        public int Priority {
            get { return -6; }
        }
    }
}