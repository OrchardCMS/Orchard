namespace Orchard.Indexing {
    public interface IIndexNotifierHandler : IEvents {
        void UpdateIndex(string indexName);
    }
}
