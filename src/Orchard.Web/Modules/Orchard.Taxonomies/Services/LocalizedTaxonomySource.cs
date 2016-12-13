using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;


namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomySource : ITaxonomySource {
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly ILocalizationService _localizationService;
        private readonly IContentManager _contentManager;

        public LocalizedTaxonomySource(ITaxonomyService taxonomyService, ITaxonomyExtensionsService taxonomyExtensionsService, ILocalizationService localizationService, IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _localizationService = localizationService;
            _contentManager = contentManager;
        }

        public TaxonomyPart GetTaxonomy(string name, ContentItem currentcontent) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }
            string culture=_localizationService.GetContentCulture(currentcontent);
            var taxonomyPart = _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
                .Join<TitlePartRecord>()
                .Where(r => r.Title == name)
                .List()
                .FirstOrDefault();
            if (String.IsNullOrWhiteSpace(culture) || _localizationService.GetContentCulture(taxonomyPart.ContentItem) == culture)
                return taxonomyPart;
            else {
                var contentItem = _localizationService.GetLocalizedContentItem(taxonomyPart.ContentItem, culture).ContentItem;
                return contentItem.As<TaxonomyPart>();
            }
        }      
    }    
}