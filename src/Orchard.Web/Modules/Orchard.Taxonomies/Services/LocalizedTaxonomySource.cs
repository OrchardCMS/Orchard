using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Events;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;

namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomySource : ITaxonomySource {

        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        public LocalizedTaxonomySource(ITaxonomyService taxonomyService, ITaxonomyExtensionsService taxonomyExtensionsService) {
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
        }
        //public IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
        //     string fieldName = field != null ? field.Name : string.Empty;
        ////    var idContentCulture = part.ContentItem.As<LocalizationPart>().Culture.Id;

        //    return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions ?? VersionOptions.Published).Distinct(new TermPartComparer());
        //}

        public TaxonomyPart GetTaxonomy(string name, ContentItem item) {
           
           return  _taxonomyExtensionsService.GetTaxonomy(name, item);
            //if (String.IsNullOrWhiteSpace(name)) {
            //    throw new ArgumentNullException("name");
            //}
            //var taxonomyPart = _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
            //    .Join<TitlePartRecord>()
            //    .Where(r => r.Title == name)
            //    .List()
            //    .FirstOrDefault();
            //var settingCulture = _localizationService.GetContentCulture(taxonomyPart.ContentItem);
            //if (settingCulture == culture)
            //    return taxonomyPart;
            //else {
            //    var contentItem = _localizationService.GetLocalizedContentItem(taxonomyPart.ContentItem, culture).ContentItem;
            //    return contentItem.As<TaxonomyPart>();
            //}
        }
        
    }
}