using Orchard.Events;

namespace Orchard.Indexing {
    public interface IIndexNotifierHandler : IEventHandler {
        void UpdateIndex(string indexName);
    }
}
