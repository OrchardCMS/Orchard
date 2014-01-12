using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Users.Models;

namespace Orchard.Users.Drivers
{
    public class RegistrationSettingsPartDriver : ContentPartDriver<RegistrationSettingsPart> {
        public RegistrationSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override void Exporting(RegistrationSettingsPart part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }
    }
}