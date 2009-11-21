namespace Orchard.Models {
    public abstract class ContentPart : IContent {
        public ContentItem ContentItem { get; set; }
    }
}
