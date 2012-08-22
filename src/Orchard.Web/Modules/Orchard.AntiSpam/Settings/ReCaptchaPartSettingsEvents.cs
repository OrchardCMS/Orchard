using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Orchard.AntiSpam.Settings {
    public class ReCaptchaPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ReCaptchaPart")
                yield break;

            var settings = definition.Settings.GetModel<ReCaptchaPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ReCaptchaPart")
                yield break;

            var settings = new ReCaptchaPartSettings {
            };

            if (updateModel.TryUpdateModel(settings, "ReCaptchaPartSettings", null, null)) {
                builder.WithSetting("ReCaptchaPartSettings.PublicKey", settings.PublicKey);
                builder.WithSetting("ReCaptchaPartSettings.PrivateKey", settings.PrivateKey);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
