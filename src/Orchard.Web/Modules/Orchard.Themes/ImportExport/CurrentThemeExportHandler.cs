using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Themes.Services;

namespace Orchard.Themes.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    [OrchardFeature("Orchard.Themes.ImportExportCurrentTheme")]
    public class CurrentThemeExportHandler : IExportEventHandler {
        private readonly ISiteThemeService _siteThemeService;
        
        public CurrentThemeExportHandler(ISiteThemeService siteThemeService) {
            _siteThemeService = siteThemeService;
        }

        public void Exporting(dynamic context) { }

        public void Exported(dynamic context) {
            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("CurrentTheme")) {
                return;
            }

            var currentThemeId = _siteThemeService.GetCurrentThemeName();
            var root = new XElement("CurrentTheme", new XAttribute("id", currentThemeId));
            context.Document.Element("Orchard").Add(root);
        }
    }
}