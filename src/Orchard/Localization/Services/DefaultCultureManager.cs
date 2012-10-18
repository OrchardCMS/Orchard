using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization.Records;

namespace Orchard.Localization.Services {
    public class DefaultCultureManager : ICultureManager {
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IEnumerable<ICultureSelector> _cultureSelectors;
        private readonly ISignals _signals;
        private readonly IWorkContextAccessor _workContextAccessor;

        public DefaultCultureManager(IRepository<CultureRecord> cultureRepository, 
                                     IEnumerable<ICultureSelector> cultureSelectors, 
                                     ISignals signals, 
                                     IWorkContextAccessor workContextAccessor) {
            _cultureRepository = cultureRepository;
            _cultureSelectors = cultureSelectors;
            _signals = signals;
            _workContextAccessor = workContextAccessor;
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
            _signals.Trigger("culturesChanged");
        }

        public void DeleteCulture(string cultureName) {
            if (!IsValidCulture(cultureName)) {
                throw new ArgumentException("cultureName");
            }

            var culture = _cultureRepository.Get(cr => cr.Culture == cultureName);
            if (culture != null) {
                _cultureRepository.Delete(culture);
                _signals.Trigger("culturesChanged");
            }
        }

        public string GetCurrentCulture(HttpContextBase requestContext) {
            var requestCulture = _cultureSelectors
                .Select(x => x.GetCulture(requestContext))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority);

            if ( requestCulture.Count() < 1 )
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

        public CultureRecord GetCultureByName(string cultureName) {
            return _cultureRepository.Get(cr => cr.Culture == cultureName);
        }

        public string GetSiteCulture() {
            return _workContextAccessor.GetContext().CurrentSite == null ? null : _workContextAccessor.GetContext().CurrentSite.SiteCulture;
        }

        // "<languagecode2>" or
        // "<languagecode2>-<country/regioncode2>" or
        // "<languagecode2>-<scripttag>-<country/regioncode2>"
        public bool IsValidCulture(string cultureName) {
            var segments = cultureName.Split('-');

            if(segments.Length == 0) {
                return false;
            }

            if (segments.Length > 3) {
                return false;
            }

            if (segments.Any(s => s.Length < 2)) {
                return false;
            }
            
            return true;
        }
    }
}
