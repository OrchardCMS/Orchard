using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Tags.Models {
    public class TagsPartRecord : ContentPartRecord {
        public TagsPartRecord() {
            Tags = new List<ContentTagRecord>();
        }

        [Aggregate]
        public virtual IList<ContentTagRecord> Tags { get; set; }
    }
}