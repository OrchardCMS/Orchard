using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Settings {
    public class LocationSettingsViewModel {
        public LocationDefinition Definition { get; set; }
        public ContentLocation Location { get; set; }
    }
}