using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Indexing.Models {
    public class IndexingTaskRecord {

        public const int Update = 0;
        public const int Delete = 1;

        public virtual int Id { get; set; }
        public virtual int Action { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}
