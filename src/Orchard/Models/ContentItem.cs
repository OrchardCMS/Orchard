using System.Collections.Generic;
using System.Linq;

namespace Orchard.Models {
    public class ContentItem {
        public ContentItem() {
            _parts = new List<IContentItemPart>();
        }

        private readonly IList<IContentItemPart> _parts;

        public int Id { get; set; }

        public string ContentType { get; set; }

        public bool Has<TPart>() {
            return _parts.Any(part => part is TPart);
        }

        public TPart Get<TPart>() {
            return _parts.OfType<TPart>().FirstOrDefault();
        }

        public void Weld(IContentItemPart part) {
            part.ContentItem = this;
            _parts.Add(part);
        }
    }
}