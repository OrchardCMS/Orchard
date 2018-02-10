using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Settings {
    public class ContentTypeLayoutSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<ContentTypeLayoutSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ContentTypeLayoutSettings();
            updateModel.TryUpdateModel(model, "ContentTypeLayoutSettings", null, null);
            builder.Placeable(model.Placeable);
            yield return DefinitionTemplate(model);
        }
    }
}