using Orchard.ContentManagement.Records;
using Orchard.Taxonomies.Models;

namespace Orchard.MediaLibrary.Models {
    public class MediaPartRecord : ContentPartRecord {
        public virtual string MimeType { get; set; }
        public virtual string Caption { get; set; }
        public virtual string AlternateText { get; set; }
        public virtual TermPartRecord TermPartRecord { get; set; }
        public virtual string Resource { get; set; }

    }
}