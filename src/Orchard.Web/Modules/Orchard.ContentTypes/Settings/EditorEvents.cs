using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.ViewModels;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Contents.Settings;

namespace Orchard.ContentTypes.Settings {
    public class EditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var settings = definition.Settings.GetModel<ContentTypeSettings>();
            var model = new ContentTypeSettingsViewModel {
                Creatable = settings.Creatable,
                Listable = settings.Listable,
                Draftable = settings.Draftable,
                Securable = settings.Securable,
            };

            if(definition.Settings.ContainsKey("Stereotype")) {
                model.Stereotype = definition.Settings["Stereotype"] ?? String.Empty;
            }

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ContentTypeSettingsViewModel();
            updateModel.TryUpdateModel(model, "ContentTypeSettingsViewModel", null, null);

            builder.Creatable(model.Creatable);
            builder.Listable(model.Listable);
            builder.Draftable(model.Draftable);
            builder.Securable(model.Securable);
            builder.WithSetting("Stereotype", model.Stereotype);

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition)
        {
            var model = definition.Settings.GetModel<ContentPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            var model = new ContentPartSettings();
            updateModel.TryUpdateModel(model, "ContentPartSettings", null, null);
            builder.Attachable(model.Attachable);
            builder.WithDescription(model.Description);
            yield return DefinitionTemplate(model);
        }
    }
}