using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Settings {
    public class CommonEditorsSettings {
        public CommonEditorsSettings() {
            // defaults
            ShowOwnerEditor = true;
            ShowDateEditor = false;
        }

        public bool ShowDateEditor { get; set; }
        public bool ShowOwnerEditor { get; set; }

        public static CommonEditorsSettings Get(ContentItem contentItem) {
            if (contentItem == null ||
                contentItem.TypeDefinition == null ||
                contentItem.TypeDefinition.Settings == null) {
                return new CommonEditorsSettings();
            }
            return contentItem.TypeDefinition.Settings.GetModel<CommonEditorsSettings>();
        }
    }

    public class CommonEditorsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            if (!definition.Parts.Any(part => part.PartDefinition.Name == typeof(CommonPart).Name)) {
                yield break;
            }

            var model = definition.Settings.GetModel<CommonEditorsSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var model = new CommonEditorsSettings();
            updateModel.TryUpdateModel(model, "CommonEditorsSettings", null, null);

            builder.WithSetting("CommonEditorsSettings.ShowDateEditor", model.ShowDateEditor.ToString());
            builder.WithSetting("CommonEditorsSettings.ShowOwnerEditor", model.ShowOwnerEditor.ToString());

            yield return DefinitionTemplate(model);
        }
    }

}
