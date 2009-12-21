namespace Orchard.ContentManagement {
    public abstract class ContentPart : IContent {
        public virtual ContentItem ContentItem { get; set; }
    }

    public class ContentPart<TRecord> : ContentPart {
        public TRecord Record { get; set; }
    }

}
