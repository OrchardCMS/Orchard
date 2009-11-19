using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public class CreateContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}