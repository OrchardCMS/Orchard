using System;

namespace Orchard.MediaLibrary.Models {
    public class MediaFile {
        public string Name { get; set; }
        public string User { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string FolderName { get; set; }
        public DateTime LastUpdated { get; set; }
        public string MediaPath { get; set; }
    }
}
