using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Orchard.Scripting.CSharp.Settings {
    [OrchardFeature("Orchard.Scripting.CSharp.Validation")]
    public class ScriptValidationPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ScriptValidationPart")
                yield break;

            var settings = definition.Settings.GetModel<ScriptValidationPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ScriptValidationPart")
                yield break;

            var settings = new ScriptValidationPartSettings();

            if (updateModel.TryUpdateModel(settings, "ScriptValidationPartSettings", null, null)) {
                builder.WithSetting("ScriptValidationPartSettings.Script", settings.Script);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
