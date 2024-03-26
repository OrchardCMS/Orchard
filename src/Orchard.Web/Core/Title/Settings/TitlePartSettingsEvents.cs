using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Title.Settings {
    public class TitlePartSettingsEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "TitlePart") {
                yield break;
            }

            var settings = definition
                .Settings
                .GetModel<TitlePartSettings>()
                ?? new TitlePartSettings();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {

            if (builder.Name != "TitlePart") {
                yield break;
            }

            var model = new TitlePartSettings();

            if (updateModel.TryUpdateModel(model, "TitlePartSettings", null, null)) {
                builder.WithSetting("TitlePartSettings.MaxLength", model.MaxLength.ToString());

            }
            yield return DefinitionTemplate(model);
        }
    }
}