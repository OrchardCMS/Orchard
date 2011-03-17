using System;
using System.Collections.Generic;

namespace Orchard.Indexing.Services {
    public class IndexEntry {
        public string IndexName { get; set; }
        public int DocumentCount { get; set; }
        public DateTime LastUpdateUtc { get; set; }
        public IEnumerable<string> Fields { get; set; }
        public IndexingStatus IndexingStatus { get; set; }
    }

    public interface IIndexingService : IDependency {
        void RebuildIndex(string indexName);
        void UpdateIndex(string indexName);
        IndexEntry GetIndexEntry(string indexName);
    }
}