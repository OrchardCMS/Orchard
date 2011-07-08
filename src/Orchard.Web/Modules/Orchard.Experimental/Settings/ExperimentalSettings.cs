using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Definition;
using Orchard.ContentManagement.Definition.Builders;
using Orchard.ContentManagement.Definition.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Experimental.Settings {
    
    public class ExperimentalSettings {
        public bool ShowDebugLinks { get; set; }
    }

    public class ExperimentalSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<ExperimentalSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ExperimentalSettings();
            updateModel.TryUpdateModel(model, "ExperimentalSettings", null, null);
            builder
                .WithSetting("ExperimentalSettings.ShowDebugLinks", model.ShowDebugLinks ? true.ToString() : null);

            yield return DefinitionTemplate(model);
        }
    }
}