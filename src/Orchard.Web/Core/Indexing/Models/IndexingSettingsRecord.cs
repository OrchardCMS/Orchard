using System;

namespace Orchard.Core.Indexing.Models {
    public class IndexingSettingsRecord {
        public virtual int Id { get; set; }
        public virtual DateTime? LatestIndexingUtc { get; set; }
    }
}