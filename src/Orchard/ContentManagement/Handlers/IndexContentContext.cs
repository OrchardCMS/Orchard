using Orchard.Indexing;

namespace Orchard.ContentManagement.Handlers {
    public class IndexContentContext {
        public ContentItem ContentItem { get; set; }
        public IIndexDocument IndexDocument { get; set; }
    }
}
