using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.AuditTrail.Settings {
    public class AuditTrailPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "AuditTrailPart")
                yield break;

            var settings = definition.Settings.GetModel<AuditTrailPartSettings>();
            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "AuditTrailPart")
                yield break;

            var settings = new AuditTrailPartSettings();
            updateModel.TryUpdateModel(settings, "AuditTrailPartSettings", null, null);
            settings.Build(builder);

            yield return DefinitionTemplate(settings);
        }
    }
}
