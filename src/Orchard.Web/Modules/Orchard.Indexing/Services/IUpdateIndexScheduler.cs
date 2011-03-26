namespace Orchard.Indexing.Services {
    public interface IUpdateIndexScheduler : IDependency {
        void Schedule(string indexName);
    }
}