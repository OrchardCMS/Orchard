using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {
    public class ContentItem : DynamicObject, IContent {
        public ContentItem() {
            _parts = new List<ContentPart>();
        }

        private readonly IList<ContentPart> _parts;

        ContentItem IContent.ContentItem { get { return this; } }

        public dynamic Content { get { return (dynamic)this; } }

        public int Id { get { return Record == null ? 0 : Record.Id; } }
        public int Version { get { return VersionRecord == null ? 0 : VersionRecord.Number; } }

        public string ContentType { get; set; }
        public ContentTypeDefinition TypeDefinition { get; set; }
        public ContentItemRecord Record { get { return VersionRecord == null ? null : VersionRecord.ContentItemRecord; } }
        public ContentItemVersionRecord VersionRecord { get; set; }

        public IEnumerable<ContentPart> Parts { get { return _parts; } }

        public IContentManager ContentManager { get; set; }

        public bool Has(Type partType) {
            return partType == typeof(ContentItem) || _parts.Any(partType.IsInstanceOfType);
        }

        public IContent Get(Type partType) {
            if (partType == typeof(ContentItem))
                return this;
            return _parts.FirstOrDefault(partType.IsInstanceOfType);
        }

        public void Weld(ContentPart part) {
            part.ContentItem = this;
            _parts.Add(part);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {

            var found = base.TryGetMember(binder, out result);
            if (!found) {
                foreach (var part in Parts) {
                    if (part.PartDefinition.Name == binder.Name) {
                        result = part;
                        return true;
                    }
                }
                result = null;
                return true;
            }

            return true;
        }
    }
}
