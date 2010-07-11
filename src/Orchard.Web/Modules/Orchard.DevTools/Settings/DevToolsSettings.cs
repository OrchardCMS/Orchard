using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.DevTools.Settings {


    public class DevToolsSettings {
        public bool ShowDebugLinks { get; set; }
    }

    public class DevToolsSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<DevToolsSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new DevToolsSettings();
            updateModel.TryUpdateModel(model, "DevToolsSettings", null, null);
            builder
                .WithSetting("DevToolsSettings.ShowDebugLinks", model.ShowDebugLinks ? true.ToString() : null);

            yield return DefinitionTemplate(model);
        }
    }
}
