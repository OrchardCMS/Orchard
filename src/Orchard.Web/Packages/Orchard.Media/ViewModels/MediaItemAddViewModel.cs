using Orchard.Mvc.ViewModels;

namespace Orchard.Media.ViewModels {
    public class MediaItemAddViewModel : AdminViewModel {
        public string FolderName { get; set; }
        public string MediaPath { get; set; }
    }
}
