using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Navigation.Settings {
    public class AdminMenuPartTypeSettings {
        public string DefaultPosition { get; set; }
    }

    public class AdminMenuSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "AdminMenuPart") {
                yield break;
            }

            var model = definition.Settings.GetModel<AdminMenuPartTypeSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "AdminMenuPart") {
                yield break;
            }

            var model = new AdminMenuPartTypeSettings();
            updateModel.TryUpdateModel(model, "AdminMenuPartTypeSettings", null, null);
            builder.WithSetting("AdminMenuPartTypeSettings.DefaultPosition", model.DefaultPosition);
            yield return DefinitionTemplate(model);
        }
    }
}
