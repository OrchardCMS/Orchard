namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorModelContext {
        public BuildEditorModelContext(IContent content) {
            ContentItem = content.ContentItem;            
        }

        public ContentItem ContentItem { get; set; }
    }
}