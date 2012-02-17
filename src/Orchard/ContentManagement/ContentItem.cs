using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using ClaySharp;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement {
    public class ContentItem : IContent, IContentBehavior, IDynamicMetaObjectProvider {
        public ContentItem() {
            _behavior = new ClayBehaviorCollection(new[] { new ContentItemBehavior(this) });
            _parts = new List<ContentPart>();
        }

        private readonly IList<ContentPart> _parts;

        ContentItem IContent.ContentItem { get { return this; } }

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


        private readonly IClayBehavior _behavior;
        IClayBehavior IContentBehavior.Behavior {
            get { return _behavior; }
        }
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter) {
            return new ClayMetaObject(this, parameter, ex => Expression.Property(Expression.Convert(ex, typeof(IContentBehavior)), "Behavior"));
        }

    }
}