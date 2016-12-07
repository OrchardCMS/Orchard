using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyExtensionsService : ITaxonomyExtensionsService {
        private readonly IAutorouteService _autorouteService;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILocalizationService _localizationService;

        public TaxonomyExtensionsService(
            IAutorouteService autorouteService,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITaxonomyService taxonomyService,
            ILocalizationService localizationService) {
            _autorouteService = autorouteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _taxonomyService = taxonomyService;
            _localizationService = localizationService;
        }

        public IEnumerable<ContentTypeDefinition> GetAllTermTypes() {
            return _contentManager.GetContentTypeDefinitions().Where(t => t.Parts.Any(p => p.PartDefinition.Name.Equals(typeof(TermPart).Name)));
        }

        public void CreateLocalizedTermContentType(TaxonomyPart taxonomy) {
            _taxonomyService.CreateTermContentType(taxonomy);
            _contentDefinitionManager.AlterTypeDefinition(taxonomy.TermTypeName,
                cfg => cfg
                    .WithPart("LocalizationPart")
                );
        }

        public ContentItem GetParentTaxonomy(TermPart part) {
            var container = _contentManager.Get(part.Container.Id);

            ContentItem parentTaxonomy = container;
            while (parentTaxonomy != null && parentTaxonomy.ContentType != "Taxonomy")
                parentTaxonomy = _contentManager.Get(parentTaxonomy.As<TermPart>().Container.Id);

            return parentTaxonomy;
        }

        public ContentItem GetParentTerm(TermPart part) {
            var container = _contentManager.Get(part.Container.Id);
            if (container.ContentType != "Taxonomy")
                return container;
            else
                return null;
        }

        public IContent GetMasterItem(IContent item) {
            if (item == null)
                return null;

            var itemLocalization = item.As<LocalizationPart>();
            if (itemLocalization == null)
                return item;
            else {
                IContent masterParentTerm = itemLocalization.MasterContentItem;
                if (masterParentTerm == null)
                    masterParentTerm = item;

                return masterParentTerm;
            }
        }

        public void RegenerateAutoroute(ContentItem item) {
            if (item.Has<AutoroutePart>()) {
                _autorouteService.RemoveAliases(item.As<AutoroutePart>());
                item.As<AutoroutePart>().DisplayAlias = _autorouteService.GenerateAlias(item.As<AutoroutePart>());
                _autorouteService.PublishAlias(item.As<AutoroutePart>());
            }
        }
        public TaxonomyPart GetTaxonomy(string name, ContentItem currentcontent) {
            if (String.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException("name");
            }
            string culture = null;
            if (currentcontent.As<LocalizationPart>() != null && currentcontent.As<LocalizationPart>().Culture != null)
                culture = currentcontent.As<LocalizationPart>().Culture.Culture;
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