using Orchard.ContentManagement.Records;
using Orchard.Logging;

namespace Orchard.ContentManagement.Handlers {
    public class ContentContextBase {
        protected ContentContextBase (ContentItem contentItem) {
            ContentItem = contentItem;
        }

        public int Id { get { return ContentItem.Id; } }
        public string ContentType { get { return ContentItem.ContentType; } }
        public ContentItem ContentItem { get; set; }
        public ContentItemRecord ContentItemRecord { get { return ContentItem.Record; } }
        public IContentManager ContentManager { get { return ContentItem.ContentManager; } }
        public ILogger Logger { get; set; }
    }
}