using System.Collections.Generic;
using Orchard.UI.Models;

namespace Orchard.Models.Driver {
    public class GetDisplaysContext {
        public GetDisplaysContext(IContent content) {
            ContentItem = content.ContentItem;
            Displays = new List<ModelTemplate>();
        }
        public ContentItem ContentItem { get; set; }
        public IList<ModelTemplate> Displays { get; set; }
    }
}