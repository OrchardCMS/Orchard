using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.Taxonomies.Services;

namespace Orchard.Taxonomies.Settings {
    public class TaxonomyFieldEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyFieldEditorEvents(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "TaxonomyField") {
                var model = GetCurrentSettings(definition);
                model.Taxonomies = _taxonomyService.GetTaxonomies().Where(tax => !tax.ContentItem.HasDraft());
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "TaxonomyField") {
                yield break;
            }

            // Init this model preventively so if the TryUpdateModel doesn't execute correctly it doesn't cause an error
            var model = GetCurrentSettings(builder.Current);

            if (updateModel.TryUpdateModel(model, "TaxonomyFieldSettings", null, null)) {
                builder
                    .WithSetting("TaxonomyFieldSettings.Taxonomy", model.Taxonomy)
                    .WithSetting("TaxonomyFieldSettings.LeavesOnly", model.LeavesOnly.ToString())
                    .WithSetting("TaxonomyFieldSettings.Required", model.Required.ToString())
                    .WithSetting("TaxonomyFieldSettings.SingleChoice", model.SingleChoice.ToString())
                    .WithSetting("TaxonomyFieldSettings.Autocomplete", model.Autocomplete.ToString())
                    .WithSetting("TaxonomyFieldSettings.AllowCustomTerms", model.AllowCustomTerms.ToString())
                    .WithSetting("TaxonomyFieldSettings.Hint", model.Hint);
            }
            
            yield return DefinitionTemplate(model);
        }

        private TaxonomyFieldSettings GetCurrentSettings(ContentPartFieldDefinition definition) {
            var model = definition.Settings.GetModel<TaxonomyFieldSettings>();
            model.Taxonomies = _taxonomyService.GetTaxonomies();
            return model;
        }

    }
}