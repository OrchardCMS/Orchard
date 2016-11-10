using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class ImportMediaViewModel {
        public string FolderPath { get; set; }
        public string Type { get; set; }
        public MediaPart Replace { get; set; }
    }
}