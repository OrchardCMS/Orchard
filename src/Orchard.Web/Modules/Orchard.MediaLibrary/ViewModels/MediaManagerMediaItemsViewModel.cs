using System.Collections.Generic;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.ViewModels {
    public class MediaManagerMediaItemsViewModel {
        public IList<MediaManagerMediaItemViewModel> MediaItems { get; set; }
        public int MediaItemsCount { get; set; }
        public string FolderPath { get; set; }
    }

    public class MediaManagerMediaItemViewModel {
        public MediaPart MediaPart { get; set; }
        public dynamic Shape { get; set; }
    }
}