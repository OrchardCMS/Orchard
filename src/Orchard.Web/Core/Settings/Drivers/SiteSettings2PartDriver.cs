using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Settings.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;

namespace Orchard.Core.Settings.Drivers
{
    public class SiteSettings2PartDriver : ContentPartDriver<SiteSettings2Part> {
        public SiteSettings2PartDriver() {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override void Exporting(SiteSettings2Part part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }

        protected override void Importing(SiteSettings2Part part, ImportContentContext context) {
            DefaultSettingsPartImportExport.ImportSettingPart(part, context.Data.Element(part.PartDefinition.Name));
        }
    }
}