using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Indexing.Models {
    public class IndexingTaskRecord {
        public virtual int Id { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}
