using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Core.ContentsLocation.ViewModels {
    public class LocationSettingsViewModel {
        public LocationDefinition Definition { get; set; }
        public ContentLocation Location { get; set; }
    }
}