using System;
using System.Globalization;
using System.Linq;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Records;
using Orchard.Mvc;

namespace Orchard.Localization.Services {
    [OrchardFeature("Orchard.Localization.CutlureSelector")]
    public class CultureService : ICultureService {
        private readonly ICultureStorageProvider _cultureStorageProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<CultureRecord> _cultureRepository;

        public CultureService(ICultureStorageProvider cultureStorageProvider,
            IHttpContextAccessor httpContextAccessor,
            IRepository<CultureRecord> cultureRepository) {
            _cultureStorageProvider = cultureStorageProvider;
            _httpContextAccessor = httpContextAccessor;
            _cultureRepository = cultureRepository;
        }

        public void SetCulture(string culture) {
            _cultureStorageProvider.SetCulture(culture);
        }

        public string GetCulture() {
            var culture = _cultureStorageProvider.GetCulture();

            if (culture == null) {
                var browserCulture = GetBrowserCulture();

                if (browserCulture == null)
                    return null;

                culture = browserCulture.Name;
            }

            return culture;
        }

        private CultureInfo GetBrowserCulture() {
            var httpContext = _httpContextAccessor.Current();

            /* Fall back to Browser */
            var userLanguages = httpContext.Request.UserLanguages;

            if (userLanguages == null || userLanguages.Length == 0)
                return null;

            var cultures = (from culture in _cultureRepository.Table select culture.Culture).ToList();

            foreach (var userLanguage in userLanguages) {
                var language = userLanguage.Split(';')[0].Trim();

                if (cultures.Contains(language, StringComparer.OrdinalIgnoreCase)) {
                    try {
                        return CultureInfo.CreateSpecificCulture(language);
                    }
                    catch (ArgumentException) {
                    }
                }
            }

            return null;
        }
    }
}