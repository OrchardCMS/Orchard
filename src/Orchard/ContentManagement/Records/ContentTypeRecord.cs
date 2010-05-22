using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Records;
using Orchard.Data.Conventions;

namespace Orchard.ContentManagement.Records {
    public class ContentTypeRecord {
        public ContentTypeRecord() {
            ContentParts = new List<ContentTypePartRecord>();
        }
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        [CascadeAllDeleteOrphan]
        public virtual IList<ContentTypePartRecord> ContentParts { get; set; }
    }
}
