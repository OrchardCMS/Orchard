using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Email.Models;
using Orchard.Localization;

namespace Orchard.Email.Drivers {

    // We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    // Thus the encryption/decryption will be done when accessing the part's property.

    public class SmtpSettingsPartDriver : ContentPartDriver<SmtpSettingsPart> {
        private const string TemplateName = "Parts/SmtpSettings";

        public SmtpSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "SmtpSettings"; } }

        protected override DriverResult Editor(SmtpSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_SmtpSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("email");
        }

        protected override DriverResult Editor(SmtpSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_SmtpSettings_Edit", () => {
                    var previousPassword = part.Password;
                    updater.TryUpdateModel(part, Prefix, null, null);

                    // Restore password if the input is empty, meaning that it has not been reset.
                    if (string.IsNullOrEmpty(part.Password)) {
                        part.Password = previousPassword;
                    }
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
                })
                .OnGroup("email");
        }
    }
}