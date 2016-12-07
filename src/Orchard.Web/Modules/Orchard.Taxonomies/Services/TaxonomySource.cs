using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public class TaxonomySource : ITaxonomySource {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;
        
        public TaxonomySource(ITaxonomyService taxonomyService, IContentManager contentManager) {
            _taxonomyService = taxonomyService;
            _contentManager = contentManager;
        }
        //public IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
        //    string fieldName = field != null ? field.Name : string.Empty;
        //    return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions ?? VersionOptions.Published).Distinct(new TermPartComparer());
        //}
        public TaxonomyPart GetTaxonomy(string name, ContentItem item) {
            return _taxonomyService.GetTaxonomyByName(name);
            //if (String.IsNullOrWhiteSpace(name)) {
            //    throw new ArgumentNullException("name");
            //}

            //// include the record in the query to optimize the query plan
            //return _contentManager.Query<TaxonomyPart, TaxonomyPartRecord>()
            //    .Join<TitlePartRecord>()
            //    .Where(r => r.Title == name)
            //    .List()
            //    .FirstOrDefault();
        }
    }
}