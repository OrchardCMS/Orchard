using System;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class LoadContentContext {
        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}