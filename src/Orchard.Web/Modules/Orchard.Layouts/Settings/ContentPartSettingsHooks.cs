using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Layouts.Helpers;

namespace Orchard.Layouts.Settings {
    public class ContentPartSettingsHooks : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            var model = definition.Settings.GetModel<ContentPartLayoutSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ContentPartLayoutSettings();
            updateModel.TryUpdateModel(model, "ContentPartLayoutSettings", null, null);
            builder.Placeable(model.Placeable);
            yield return DefinitionTemplate(model);
        }
    }
}