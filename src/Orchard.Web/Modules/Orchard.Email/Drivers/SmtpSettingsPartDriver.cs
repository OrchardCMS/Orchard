using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Email.Models;
using Orchard.Localization;

namespace Orchard.Email.Drivers {

    // We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    // Thus the encryption/decryption will be done when accessing the part's property

    public class SmtpSettingsPartDriver : ContentPartDriver<SmtpSettingsPart> {
        public SmtpSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "SmtpSettings"; } }

        protected override DriverResult Editor(SmtpSettingsPart part) {
            return ContentPartTemplate(part, "Parts/Smtp.SiteSettings");
        }

        protected override DriverResult Editor(SmtpSettingsPart part, IUpdateModel updater) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part);
        }
    }
}