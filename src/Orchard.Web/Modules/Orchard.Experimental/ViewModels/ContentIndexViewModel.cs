using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Experimental.ViewModels {

    public class ContentIndexViewModel  {
        public IEnumerable<ContentTypeRecord> Types { get; set; }
        public IEnumerable<ContentItem> Items { get; set; }
    }
}
