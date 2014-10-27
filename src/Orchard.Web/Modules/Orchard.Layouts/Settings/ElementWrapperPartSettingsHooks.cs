using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Layouts.Settings {
    public class ElementWrapperPartSettingsHooks : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ElementWrapperPart")
                yield break;

            var model = definition.Settings.GetModel<ElementWrapperPartSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ElementWrapperPart")
                yield break;

            var model = new ElementWrapperPartSettings();
            updateModel.TryUpdateModel(model, "ElementWrapperPartSettings", null, null);
            builder.WithSetting("ElementWrapperPartSettings.ElementTypeName", model.ElementTypeName);
            yield return DefinitionTemplate(model);
        }
    }
}