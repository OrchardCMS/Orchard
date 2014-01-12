using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.Settings;

namespace Orchard.MediaLibrary.Drivers
{
    public class WebSearchSettingsPartDriver : ContentPartDriver<WebSearchSettingsPart> {
        public WebSearchSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        protected override void Exporting(WebSearchSettingsPart part, ExportContentContext context) {
            DefaultSettingsPartImportExport.ExportSettingsPart(part, context);
        }

        protected override void Importing(WebSearchSettingsPart part, ImportContentContext context) {
            DefaultSettingsPartImportExport.ImportSettingPart(part, context.Data.Element(part.PartDefinition.Name));
        }
    }
}