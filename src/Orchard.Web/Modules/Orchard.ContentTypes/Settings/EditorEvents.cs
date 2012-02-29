using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Contents.Settings;

namespace Orchard.ContentTypes.Settings {
    public class EditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            var model = definition.Settings.GetModel<ContentTypeSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new ContentTypeSettings();
            updateModel.TryUpdateModel(model, "ContentTypeSettings", null, null);
            builder.Creatable(model.Creatable);
            builder.Draftable(model.Draftable);

            yield return DefinitionTemplate(model);
        }
    }
}