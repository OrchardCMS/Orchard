namespace Orchard.ContentManagement.Handlers {
    public class BuildDisplayModelContext {
        public BuildDisplayModelContext(IContent content, string displayType) {
            ContentItem = content.ContentItem;            
            DisplayType = displayType;
        }

        public ContentItem ContentItem { get; private set; }
        public string DisplayType { get; private set; }
    }
}
