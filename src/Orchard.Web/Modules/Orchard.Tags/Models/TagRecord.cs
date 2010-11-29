using System.Collections.Generic;

namespace Orchard.Tags.Models {
    public class TagRecord {
        public TagRecord() {
            ContentTags = new List<ContentTagRecord>();
        }
        public virtual int Id { get; set; }
        public virtual string TagName { get; set; }
        public virtual IList<ContentTagRecord> ContentTags { get; set; }
    }
}
