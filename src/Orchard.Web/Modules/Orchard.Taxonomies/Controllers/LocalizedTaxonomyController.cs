using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
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

        [OutputCache(NoStore = true, Duration = 0)]
        public virtual ActionResult GetTaxonomy(string contentTypeName, string taxonomyFieldName, int contentId, string culture, string selectedValues) {
            return GetTaxonomyInternal(contentTypeName, taxonomyFieldName, contentId, culture, selectedValues);
        }

        protected ActionResult GetTaxonomyInternal(string contentTypeName, string taxonomyFieldName, int contentId, string culture, string selectedValues) {
            var viewModel = new TaxonomyFieldViewModel();
            bool autocomplete = false;
            var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentTypeName);
            if (contentDefinition != null) {
                var taxonomyField = contentDefinition.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField" && x.Name == taxonomyFieldName).FirstOrDefault();
                var contentTypePartDefinition = contentDefinition.Parts.Where(x => x.PartDefinition.Fields.Any(a => a.FieldDefinition.Name == "TaxonomyField" && a.Name == taxonomyFieldName)).FirstOrDefault();
                var fieldPrefix = contentTypePartDefinition.PartDefinition.Name + "." + taxonomyField.Name;
                ViewData.TemplateInfo.HtmlFieldPrefix = fieldPrefix;
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
                    int firstTermIdForCulture = 0;
                    if (contentId > 0) {
                        var selectedIds = selectedValues.Split(',');
                        var destinationTaxonomyCulture = taxonomy.As<LocalizationPart>()?.Culture?.Culture;
                        foreach (var id in selectedIds) {
                            if (!string.IsNullOrWhiteSpace(id)) {
                                var intId = 0;
                                int.TryParse(id, out intId);
                                var originalTerm = _taxonomyService.GetTerm(intId);

                                // The original term has to be added to applied terms in the following scenarios:
                                // When the original term has no LocalizationPart, which means that, when creating the taxonomy, terms have been set to be culture neutral.
                                // When the culture of the original term matches the culture of the taxonomy.
                                // In any other scenario, get the localized term and add it to the applied terms list.
                                // If no localization is found, nothing is added to the list for the current id.
                                var otCulture = originalTerm.As<LocalizationPart>()?.Culture?.Culture;
                                if (!originalTerm.Has<LocalizationPart>() || string.Equals(destinationTaxonomyCulture, otCulture)) {
                                    appliedTerms.Add(originalTerm);
                                } else {
                                    // Get the localized term. If no localized term is found, no term should be added to applied terms list.
                                    var t = _localizationService.GetLocalizedContentItem(originalTerm, culture);
                                    if (t != null) {
                                        // Localized term has been found
                                        appliedTerms.Add(t.As<TermPart>());
                                    }
                                }

                            }
                        }

                        // It takes the first term localized with the culture in order to set correctly the TaxonomyFieldViewModel.SingleTermId
                        var firstTermForCulture = appliedTerms.FirstOrDefault(x => x.As<LocalizationPart>() != null && x.As<LocalizationPart>().Culture != null && x.As<LocalizationPart>().Culture.Culture == culture);
                        if (firstTermForCulture != null) {
                            firstTermIdForCulture = firstTermForCulture.Id;
                        } else {
                            // If there is no valid localization, firstTermForCulture is null.
                            // To avoid that, use the first checked term (if any is checked).
                            firstTermForCulture = appliedTerms.FirstOrDefault(t => terms.Any(x => x.Id == t.Id));
                            if (firstTermForCulture != null) {
                                firstTermIdForCulture = firstTermForCulture.Id;
                            }
                        }
                        terms.ForEach(t => t.IsChecked = appliedTerms.Any(x => x.Id == t.Id));
                    }
                    viewModel = new TaxonomyFieldViewModel {
                        DisplayName = taxonomyField.DisplayName,
                        Name = taxonomyField.Name,
                        Terms = terms,
                        SelectedTerms = appliedTerms,
                        Settings = taxonomySettings,
                        SingleTermId = firstTermIdForCulture,
                        TaxonomyId = taxonomy != null ? taxonomy.Id : 0,
                        HasTerms = taxonomy != null && _taxonomyService.GetTermsCount(taxonomy.Id) > 0
                    };
                    if (taxonomySettings.Autocomplete) {
                        autocomplete = true;
                    }
                }
            }
            var templateName = autocomplete ? "../EditorTemplates/Fields/TaxonomyField.Autocomplete" : "../EditorTemplates/Fields/TaxonomyField";
            return PartialView(templateName, viewModel);
        }
    }
}
