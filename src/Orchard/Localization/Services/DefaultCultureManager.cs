using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Orchard.Data;
using Orchard.Localization.Records;

namespace Orchard.Localization.Services {
    public class DefaultCultureManager : ICultureManager {
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IEnumerable<ICultureSelector> _cultureSelectors;

        public DefaultCultureManager(IRepository<CultureRecord> cultureRepository, IEnumerable<ICultureSelector> cultureSelectors) {
            _cultureRepository = cultureRepository;
            _cultureSelectors = cultureSelectors;
        }

        public IEnumerable<string> ListCultures() {
            var query = from culture in _cultureRepository.Table select culture.Culture;
            return query.ToList();
        }

        public void AddCulture(string cultureName) {
            if (!IsValidCulture(cultureName)) {
                throw new ArgumentException("cultureName");
            }
            _cultureRepository.Create(new CultureRecord { Culture = cultureName });
        }

        public string GetCurrentCulture(HttpContext requestContext) {
            var requestCulture = _cultureSelectors
                .Select(x => x.GetCulture(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority);

            if (requestCulture.Count() < 1)
                return String.Empty;

            foreach (var culture in requestCulture) {
                if (!String.IsNullOrEmpty(culture.CultureName)) {
                    return culture.CultureName;
                }
            }

            return String.Empty;
        }

        public CultureRecord GetCultureById(int id) {
            return _cultureRepository.Get(id);
        }

        // "<languagecode2>" or
        // "<languagecode2>-<country/regioncode2>" or
        // "<languagecode2>-<scripttag>-<country/regioncode2>"
        private static bool IsValidCulture(string cultureName) {
            Regex cultureRegex = new Regex(@"\w{2}(-\w{2,})*");
            if (cultureRegex.IsMatch(cultureName)) {
                return true;
            }
            return false;
        }
    }
}
