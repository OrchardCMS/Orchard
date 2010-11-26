using System.Collections.Generic;

namespace Orchard.Tags.Models {
    public class TagRecord {
        public virtual int Id { get; set; }
        public virtual string TagName { get; set; }
        public virtual IList<ContentTagRecord> ContentTags { get; set; }
    }
}
