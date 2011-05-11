using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Common.Settings {
    public class CommonTypePartSettings {
        public bool ShowCreatedUtcEditor { get; set; }
    }

    public class CommonSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CommonPart")
                yield break;

            var model = definition.Settings.GetModel<CommonTypePartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CommonPart")
                yield break;

            var model = new CommonTypePartSettings();
            updateModel.TryUpdateModel(model, "CommonTypePartSettings", null, null);
            builder.WithSetting("CommonTypePartSettings.ShowCreatedUtcEditor", model.ShowCreatedUtcEditor.ToString());
            yield return DefinitionTemplate(model);
        }
    }
}