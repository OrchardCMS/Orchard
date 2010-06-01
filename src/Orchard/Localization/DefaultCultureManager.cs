using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Data;
using Orchard.Localization.Records;

namespace Orchard.Localization {
    public class DefaultCultureManager : ICultureManager {
        private readonly IRepository<CultureRecord> _cultureRepository;

        public DefaultCultureManager(IRepository<CultureRecord> cultureRepository) {
            _cultureRepository = cultureRepository;
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
