using System;

namespace Orchard.Media.Models { 
    public class MediaFolder {
        public string Name { get; set; }
        public string MediaPath { get; set; }
        public string User { get; set; }
        public long Size { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
