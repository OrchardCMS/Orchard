using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public class CreateModelContext {
        public int Id { get; set; }
        public string ModelType { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}