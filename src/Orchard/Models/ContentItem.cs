using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Models.Records;

namespace Orchard.Models {
    public class ContentItem : IContent {
        public ContentItem() {
            _parts = new List<ContentPart>();
        }


        private readonly IList<ContentPart> _parts;
        ContentItem IContent.ContentItem { get { return this; } }

        public int Id { get; set; }
        public string ContentType { get; set; }
        public ContentItemRecord Record { get; set; }

        public IEnumerable<ContentPart> Parts { get { return _parts; } }


        public bool Has(Type partType) {
            return _parts.Any(part => partType.IsAssignableFrom(part.GetType()));
        }

        public IContent Get(Type partType) {
            return _parts.FirstOrDefault(part => partType.IsAssignableFrom(part.GetType()));
        }

        public void Weld(ContentPart part) {
            part.ContentItem = this;
            _parts.Add(part);
        }
    }
}