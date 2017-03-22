using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Controllers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyController : Controller {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizedTaxonomyController(
                IContentDefinitionManager contentDefinitionManager,
                ILocalizationService localizationService,
                ITaxonomyService taxonomyService,
                ITaxonomyExtensionsService taxonomyExtensionsService) {
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _contentDefinitionManager = contentDefinitionManager;
            _localizationService = localizationService;
        }

        public ActionResult GetTaxonomy(string contentTypeName, string taxonomyFieldName, int contentId, string culture) {
            var viewModel = new TaxonomyFieldViewModel();
            bool autocomplete = false;
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeName);
            if (contentDefinition != null) {
                var taxonomyField = contentDefinition.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField" && x.Name == taxonomyFieldName).FirstOrDefault();
                var contentTypePartDefinition = contentDefinition.Parts.Where(x => x.PartDefinition.Fields.Any(a => a.FieldDefinition.Name == "TaxonomyField" && a.Name == taxonomyFieldName)).FirstOrDefault();
                ViewData.TemplateInfo.HtmlFieldPrefix = contentTypePartDefinition.PartDefinition.Name + "." + taxonomyField.Name;
                if (taxonomyField != null) {
                    var taxonomySettings = taxonomyField.Settings.GetModel<TaxonomyFieldSettings>();
                    // Getting the translated taxonomy and its terms

                    var masterTaxonomy = _taxonomyExtensionsService.GetMasterItem(_taxonomyService.GetTaxonomyByName(taxonomySettings.Taxonomy));
                    IContent taxonomy;
                    var trytranslate = _localizationService.GetLocalizedContentItem(masterTaxonomy, culture);
                    if (trytranslate == null) // case taxonomy not localized
                        taxonomy = masterTaxonomy;
                    else
                        taxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, culture).ContentItem;
                    var terms = taxonomy != null // && !taxonomySettings.Autocomplete
                        ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).Where(te => !te.HasDraft).ToList()
                        : new List<TermEntry>(0);
                    List<TermPart> appliedTerms = new List<TermPart>();
                    if (contentId > 0) {
                        appliedTerms = _taxonomyService.GetTermsForContentItem(contentId, taxonomyFieldName, VersionOptions.Published).Distinct(new TermPartComparer()).ToList();
                        terms.ForEach(t => t.IsChecked = appliedTerms.Select(x => x.Id).Contains(t.Id));
                    }
                    viewModel = new TaxonomyFieldViewModel {
                        DisplayName = taxonomyField.DisplayName,
                        Name = taxonomyField.Name,
                        Terms = terms,
                        SelectedTerms = appliedTerms,
                        Settings = taxonomySettings,
                        SingleTermId = appliedTerms.Select(t => t.Id).FirstOrDefault(),
                        TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                        HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
                    };
                    if (taxonomySettings.Autocomplete)
                        autocomplete = true;
                }
            }
            var templateName = autocomplete ? "../EditorTemplates/Fields/TaxonomyField.Autocomplete" : "../EditorTemplates/Fields/TaxonomyField";
            return View(templateName, viewModel);
        }
        private IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null) {
            string fieldName = field != null ? field.Name : string.Empty;
            return _taxonomyService.GetTermsForContentItem(part.ContentItem.Id, fieldName, versionOptions ?? VersionOptions.Published).Distinct(new TermPartComparer());
        }
    }
}