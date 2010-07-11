using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class SaveContentContext : ContentContextBase {
        public SaveContentContext(ContentItem contentItem)
            : base(contentItem) {
            ContentItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}
