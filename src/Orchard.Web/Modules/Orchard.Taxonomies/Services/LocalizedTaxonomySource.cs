using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;


namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomySource : ITaxonomySource {
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;
        public LocalizedTaxonomySource(
            ILocalizationService localizationService,
            IContentManager contentManager) {
            _localizationService = localizationService;
            _contentManager = contentManager;
        }

        public TaxonomyPart GetTaxonomy(string name, ContentItem currentcontent) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }
            string culture = _localizationService.GetContentCulture(currentcontent);
            var taxonomyPart = _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
            if (String.IsNullOrWhiteSpace(culture) || _localizationService.GetContentCulture(taxonomyPart.ContentItem) == culture)
                return taxonomyPart;
            else {
                // correction for property MasterContentItem=null for contentitem master
                var masterCorrection = taxonomyPart.ContentItem.As<LocalizationPart>().MasterContentItem;
                if (masterCorrection == null)
                    masterCorrection = taxonomyPart;
                var localizedLocalizationPart = _localizationService.GetLocalizedContentItem(masterCorrection.ContentItem, culture);
                if (localizedLocalizationPart == null)
                    return taxonomyPart;
                else
                    return localizedLocalizationPart.ContentItem.As<TaxonomyPart>();
            }
        }
    }
}