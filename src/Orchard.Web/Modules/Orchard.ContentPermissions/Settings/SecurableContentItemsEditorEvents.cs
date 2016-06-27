using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentPermissions.ViewModels;

namespace Orchard.ContentPermissions.Settings {
    public class SecurableContentItemsEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var settings = definition.Settings.GetModel<ContentPermissionsTypeSettings>();
            var model = new SecurableContentItemsSettingsViewModel {
                SecurableContentItems = settings.SecurableContentItems,
            };

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new SecurableContentItemsSettingsViewModel();
            updateModel.TryUpdateModel(model, "SecurableContentItemsSettingsViewModel", null, null);

            builder.WithSetting("ContentPermissionsTypeSettings.SecurableContentItems", model.SecurableContentItems.ToString());

            yield return DefinitionTemplate(model);
        }
    }
}