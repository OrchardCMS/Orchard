using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.Projections.Descriptors.Layout {
    public class LayoutComponentResult {
        public ContentItem ContentItem { get; set; }
        public ContentItemMetadata ContentItemMetadata { get; set; }
        public dynamic Properties { get; set; }
    }
}