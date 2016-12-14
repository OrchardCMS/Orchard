using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;
using Orchard.Taxonomies.Helpers;
using Orchard.Localization.Services;
using Orchard.Localization.Models;

using Orchard.Localization;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private readonly ILocalizationService _localizationService;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ITaxonomyExtensionsService _taxonomyExtensionsService;
        private readonly ICultureManager _cultureManager;
        private readonly IContentManager _contentManager;
        public LocalizedTaxonomyFieldDriver(ILocalizationService localizationService, ITaxonomyService taxonomyService, ITaxonomyExtensionsService taxonomyExtensionsService, ICultureManager cultureManager, IContentManager contentManager) {
            _localizationService = localizationService;
            _taxonomyService = taxonomyService;
            _taxonomyExtensionsService = taxonomyExtensionsService;
            _cultureManager = cultureManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            return ContentShape("Fields_TaxonomyFieldList_Edit", GetDifferentiator(field, part), () => {
                string selectedCulture;
                var settings = field.PartFieldDefinition.Settings.GetModel<TaxonomyFieldSettings>();
                if (part.ContentItem.As<LocalizationPart>() != null) {
                    if (part.ContentItem.As<LocalizationPart>().Culture == null) {

                        selectedCulture = _cultureManager.GetSiteCulture();//for new contentItem with localization
                    }
                    else
                        selectedCulture = part.ContentItem.As<LocalizationPart>().Culture.Culture;
                }
                else {

                    var taxonomyPart = _taxonomyService.GetTaxonomyByName(settings.Taxonomy);
                    if (taxonomyPart.ContentItem.As<LocalizationPart>() != null)
                        selectedCulture = taxonomyPart.ContentItem.As<LocalizationPart>().Culture.Culture;
                    else
                        selectedCulture = null; // CI without localization and taxonomy without localization
                }
                var localizationSetting = new LocalizationTaxonomyFieldSettings {
                    Taxonomy = settings.Taxonomy,
                    Hint = settings.Hint,
                    LeavesOnly = settings.LeavesOnly,
                    Required = settings.Required,
                    SingleChoice = settings.SingleChoice,
                    Autocomplete = settings.Autocomplete
                };

                var templateName = "Fields/TaxonomyFieldList"; 
                var viewModel = new LocalizationTaxonomiesViewModel {
                    Cultures = _cultureManager.ListCultures(),
                    SelectedCulture = selectedCulture,
                    Settings = localizationSetting,
                    ContentType = part.ContentItem.ContentType,
                    ContentPart = part.PartDefinition.Name,
                    FieldName = field.Name,
                    Prefix = GetPrefix(field, part),
                    Id = part.ContentItem.Id
                    //,Prefix = "Localized_Taxonomy_" + contentItemTaxonomyMaster.Id
                };
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
            });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, field, shapeHelper);
       }

        private static string GetDifferentiator(TaxonomyField field, ContentPart part) {
            return field.Name;
        }
     
    }
}