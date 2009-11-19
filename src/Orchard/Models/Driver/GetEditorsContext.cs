using System.Collections.Generic;
using Orchard.UI.Models;

namespace Orchard.Models.Driver {
    public class GetEditorsContext {
        public GetEditorsContext(ContentItem part) {
            ContentItem = part;
            Editors= new List<ModelTemplate>();
        }
        public ContentItem ContentItem { get; set; }
        public IList<ModelTemplate> Editors { get; set; }
    }

    public class GetDisplaysContext {
        public GetDisplaysContext(ContentItem part) {
            ContentItem = part;
            Displays = new List<ModelTemplate>();
        }
        public ContentItem ContentItem { get; set; }
        public IList<ModelTemplate> Displays { get; set; }
    }
}