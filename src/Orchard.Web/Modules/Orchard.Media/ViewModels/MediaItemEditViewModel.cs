using System;

namespace Orchard.Media.ViewModels {
    public class MediaItemEditViewModel {
        public string Name { get; set; }
        public string Caption { get; set; }
        public long Size { get; set; }
        public DateTime LastUpdated { get; set; }
        public string FolderName { get; set; }
        public string MediaPath { get; set; }
        public string RelativePath {
            get {
                return MediaPath.Replace("\\", "/");
            }
        }

        public string PublicUrl { get; set; }
    }
}
