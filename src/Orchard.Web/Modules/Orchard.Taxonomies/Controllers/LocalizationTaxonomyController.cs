using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Taxonomies.Drivers;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Helpers;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Controllers {
    public class LocalizationTaxonomyController : Controller {

        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;

        public LocalizationTaxonomyController(
            IContentDefinitionManager contentDefinitionManager,
                ILocalizationService localizationService,
                ITaxonomyService taxonomyService,
                ITaxonomyExtensionsService taxonomyExtensionsService) {
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _contentDefinitionManager = contentDefinitionManager;
            _localizationService = localizationService;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        public ActionResult GetTaxonomy(string contentTypeName, string taxonomyFieldName, string culture) {

            var viewModel = new TaxonomyFieldViewModel();
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeName);

            if (contentDefinition != null) {
                // Getting the TaxonomyField
                var taxonomyFields = contentDefinition.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField");
                var taxonomyField = taxonomyFields.Where(x => x.Name == taxonomyFieldName).FirstOrDefault();

                var contentPart = contentDefinition.Parts.Where(x => x.PartDefinition.Fields.Any(a => a.FieldDefinition.Name == "TaxonomyField" && a.Name == taxonomyFieldName)).FirstOrDefault();
                ViewData.TemplateInfo.HtmlFieldPrefix = contentPart.PartDefinition.Name + "." + taxonomyField.Name;

                if (taxonomyField != null) {
                    var taxonomySettings = taxonomyField.Settings.GetModel<TaxonomyFieldSettings>();

                    // Getting the translated taxonomy and its terms
                    var masterTaxonomy = _taxonomyExtensionsService.GetMasterItem(_taxonomyService.GetTaxonomyByName(taxonomySettings.Taxonomy));
                    var taxonomy = _localizationService.GetLocalizedContentItem(masterTaxonomy, culture);
                    var terms = taxonomy != null && !taxonomySettings.Autocomplete
                        ? _taxonomyService.GetTerms(taxonomy.Id).Where(t => !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.CreateTermEntry()).ToList()
                        : new List<TermEntry>(0);

                    TaxonomyFieldSettings tfs = new TaxonomyFieldSettings {
                        Taxonomy = taxonomySettings.Taxonomy,
                        Hint = taxonomySettings.Hint,
                        LeavesOnly = taxonomySettings.LeavesOnly,
                        Required = taxonomySettings.Required,
                        SingleChoice = taxonomySettings.SingleChoice,
                        Autocomplete = taxonomySettings.Autocomplete
                    };

                    viewModel = new TaxonomyFieldViewModel {
                        DisplayName = taxonomyField.Name,
                        Name = taxonomyField.Name,
                        Terms = terms,
                        SelectedTerms = new List<TermPart>(),
                        Settings = tfs,
                        SingleTermId = 0,
                        TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                        HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
                    };
                }
            }

            return View("TaxonomyField", viewModel);
        }
    }
}