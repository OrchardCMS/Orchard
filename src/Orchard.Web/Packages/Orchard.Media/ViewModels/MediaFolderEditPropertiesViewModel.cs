using Orchard.Mvc.ViewModels;

namespace Orchard.Media.ViewModels {
    public class MediaFolderEditPropertiesViewModel : AdminViewModel {
        public string Name { get; set; }
        public string MediaPath { get; set; }
    }
}
