using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.Records;

namespace Orchard.Models {
    public class ContentItem {
        public ContentItem() {
            _parts = new List<IContentItemPart>();
        }


        private readonly IList<IContentItemPart> _parts;

        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItemRecord ContentItemRecord { get; set; }

        public IEnumerable<IContentItemPart> Parts { get { return _parts; } }


        public bool Has(Type partType) {
            return _parts.Any(part => partType.IsAssignableFrom(part.GetType()));
        }

        public IContentItemPart Get(Type partType) {
            return _parts.FirstOrDefault(part => partType.IsAssignableFrom(part.GetType()));
        }

        public void Weld(IContentItemPart part) {
            part.ContentItem = this;
            _parts.Add(part);
        }
    }
}