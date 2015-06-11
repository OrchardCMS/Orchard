using Orchard.ContentManagement;

namespace Orchard.Core.Contents.ViewModels {
    public class PublishContentViewModel {
        public PublishContentViewModel(ContentItem contentItem) {
            ContentItem = contentItem;
        }

        public ContentItem ContentItem { get; private set; }

        public bool IsPublished {
            get { return ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published; }
        }

        public bool HasDraft {
            get {
                return (
                    (ContentItem.VersionRecord != null)
                    && ((ContentItem.VersionRecord.Published == false)
                        || (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false)));
            }
        }

        public bool HasPublished {
            get { return IsPublished || ContentItem.ContentManager.Get(ContentItem.Id, VersionOptions.Published) != null; }
        }
    }
}