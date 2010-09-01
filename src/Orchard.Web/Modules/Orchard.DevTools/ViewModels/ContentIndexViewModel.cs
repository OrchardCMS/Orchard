using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.DevTools.ViewModels {
    public class ContentIndexViewModel  {
        public IEnumerable<ContentTypeRecord> Types { get; set; }
        public IEnumerable<ContentItem> Items { get; set; }
    }
}
