using System;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class LoadContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItem ContentItem { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }

    public class RemoveContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItem ContentItem { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
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
