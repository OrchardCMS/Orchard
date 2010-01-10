using System;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {

    public class ContentContextBase {
        protected ContentContextBase (ContentItem contentItem) {
            ContentItem = contentItem;
            Id = contentItem.Id;
            ContentType = contentItem.ContentType;
            ContentItemRecord = contentItem.Record;
        }

        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItem ContentItem { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
    }

    public class LoadContentContext : ContentContextBase {
        public LoadContentContext(ContentItem contentItem) : base(contentItem) {
            ContentItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }

    public class PublishContentContext : ContentContextBase {
        public PublishContentContext(ContentItem contentItem, ContentItemVersionRecord previousItemVersionRecord) : base(contentItem) {
            PublishingItemVersionRecord = contentItem.VersionRecord;
            PreviousItemVersionRecord = previousItemVersionRecord;
        }

        public ContentItemVersionRecord PublishingItemVersionRecord { get; set; }
        public ContentItemVersionRecord PreviousItemVersionRecord { get; set; }
    }

    public class RemoveContentContext : ContentContextBase {
        public RemoveContentContext(ContentItem contentItem) : base(contentItem) {
        }
    }

    public class VersionContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }

        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItemVersionRecord ExistingItemVersionRecord { get; set; }
        public ContentItemVersionRecord BuildingItemVersionRecord { get; set; }

        public ContentItem ExistingContentItem { get; set; }
        public ContentItem BuildingContentItem { get; set; }
    }


}
