using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Taxonomies.Settings {
    public class TermPartEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "TermPart") {
                var model = definition.Settings.GetModel<TermPartSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "TermPart") {
                yield break;
            }

            var model = new TermPartSettings();

            if (updateModel.TryUpdateModel(model, "TermPartSettings", null, null)) {
                builder
                    .WithSetting("TermPartSettings.ChildDisplayType", model.ChildDisplayType);
            }

            yield return DefinitionTemplate(model);
        }
    }
}