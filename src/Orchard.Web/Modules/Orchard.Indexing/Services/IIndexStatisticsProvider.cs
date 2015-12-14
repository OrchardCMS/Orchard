using System;

namespace Orchard.Indexing.Services {
    public enum IndexingStatus {
        Rebuilding,
        Updating,
        Idle,
        Unavailable
    }
    public interface IIndexStatisticsProvider : IDependency {
        DateTime GetLastIndexedUtc(string indexName);
        IndexingStatus GetIndexingStatus(string indexName);
    }
}