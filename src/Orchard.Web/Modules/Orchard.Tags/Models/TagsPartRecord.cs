using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.Tags.Models {
    public class TagsPartRecord : ContentPartRecord {
        public TagsPartRecord() {
            Tags = new List<ContentTagRecord>();
        }

        public virtual IList<ContentTagRecord> Tags { get; set; }
    }
}