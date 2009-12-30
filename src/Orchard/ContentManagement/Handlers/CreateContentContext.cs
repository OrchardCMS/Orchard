using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class CreateContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }

        public ContentItem ContentItem { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}
