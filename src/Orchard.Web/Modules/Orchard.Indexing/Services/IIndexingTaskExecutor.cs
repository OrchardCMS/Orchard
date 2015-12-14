namespace Orchard.Indexing.Services {
    public interface IIndexingTaskExecutor : IDependency {
        bool DeleteIndex(string indexName);
        bool RebuildIndex(string indexName);
        bool UpdateIndexBatch(string indexName);
    }
}