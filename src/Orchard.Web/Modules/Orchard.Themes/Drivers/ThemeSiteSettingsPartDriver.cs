using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Themes.Drivers
{
    public class ThemeSiteSettingsPartDriver : ContentPartDriver<ThemeSiteSettingsPart> {
        public ThemeSiteSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override void Exporting(ThemeSiteSettingsPart part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }

        protected override void Importing(ThemeSiteSettingsPart part, ImportContentContext context) {
            DefaultSettingsPartImportExport.ImportSettingPart(part, context.Data.Element(part.PartDefinition.Name));
        }
    }
}