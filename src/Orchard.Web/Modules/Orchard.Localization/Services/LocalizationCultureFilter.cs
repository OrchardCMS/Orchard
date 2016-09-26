using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;

namespace Orchard.Localization.Services {
    [OrchardSuppressDependency("Orchard.Localization.Services.DefaultCultureFilter")]
    public class LocalizationCultureFilter : ICultureFilter {
        private readonly ICultureManager _cultureManager;

        public LocalizationCultureFilter(ICultureManager cultureManager) {
            _cultureManager = cultureManager;
        }

        public IContentQuery<ContentItem> FilterCulture(IContentQuery<ContentItem> query, string cultureName) {
            var culture = _cultureManager.GetCultureByName(cultureName);
            
            if (culture != null) {
                return query.Where<LocalizationPartRecord>(x => x.CultureId == culture.Id);
            }

            return query;
        }
    }
}
