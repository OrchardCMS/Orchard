using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Settings {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public class LocalizationCultureNeutralityEditorEvents : ContentDefinitionEditorEventsBase {
        //The CultureNeutral Setting may be attached to either parts or fields.
        //The CultureNeutral Setting only makes sense if the ContentType can be localized.
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private bool _typeHasLocalizationPart { get; set; }
        public LocalizationCultureNeutralityEditorEvents(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (_typeHasLocalizationPart) {
                var settings = definition.Settings.GetModel<LocalizationCultureNeutralitySettings>();
                yield return DefinitionTemplate(settings);
            }
        }
        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.PartName);
            if (typeDefinition != null && typeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "LocalizationPart")) {
                _typeHasLocalizationPart = true;
                var settings = new LocalizationCultureNeutralitySettings();
                if (updateModel.TryUpdateModel(settings, "LocalizationCultureNeutralitySettings", null, null)) {
                    settings.BuildSettings(builder);
                }
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.ContentTypeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "LocalizationPart")) {
                _typeHasLocalizationPart = true;
                var settings = definition.Settings.GetModel<LocalizationCultureNeutralitySettings>();
                yield return DefinitionTemplate(settings);
            }
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            if (typeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "LocalizationPart")) {
                _typeHasLocalizationPart = true;
                var settings = new LocalizationCultureNeutralitySettings();
                if (updateModel.TryUpdateModel(settings, "LocalizationCultureNeutralitySettings", null, null)) {
                    settings.BuildSettings(builder);
                }
                yield return DefinitionTemplate(settings);
            }
        }
    }
}