using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.Settings;

namespace Orchard.OutputCache.Drivers
{
    public class CacheSettingsPartDriver : ContentPartDriver<CacheSettingsPart> {
        public CacheSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override void Exporting(CacheSettingsPart part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }

        protected override void Importing(CacheSettingsPart part, ImportContentContext context) {
            DefaultSettingsPartImportExport.ImportSettingPart(part, context.Data.Element(part.PartDefinition.Name));
        }
    }
}