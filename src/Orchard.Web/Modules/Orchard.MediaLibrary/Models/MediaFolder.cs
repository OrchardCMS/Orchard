using System.Collections.Generic;
using System.Linq;

namespace Orchard.MediaLibrary.Models { 
    public class MediaFolder {
        public MediaFolder() {
            Folders = new List<MediaFolder>();
        }

        public string Name { get; set; }
        public string MediaPath { get; set; }
        public int TermId { get; set; }
        public int? ParentTermId { get; set; }

        public IList<MediaFolder> Folders { get; set; }
    }
}
