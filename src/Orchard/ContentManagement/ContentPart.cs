using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement {
    public abstract class ContentPart : IContent {
        private readonly IList<ContentField> _fields;

        public ContentPart() {
            _fields = new List<ContentField>();
        }

        public virtual ContentItem ContentItem { get; set; }
        public ContentTypeDefinition TypeDefinition { get { return ContentItem.TypeDefinition; } }
        public ContentTypeDefinition.Part TypePartDefinition { get; set; }
        public ContentPartDefinition PartDefinition { get { return TypePartDefinition.PartDefinition; } }

        public IEnumerable<ContentField> Fields { get { return _fields; } }


        public bool Has(Type fieldType) {
            return fieldType == typeof(ContentItem) || _fields.Any(field => fieldType.IsAssignableFrom(field.GetType()));
        }

        public IContent Get(Type fieldType) {
            if (fieldType == typeof(ContentItem))
                return this;
            return _fields.FirstOrDefault(field => fieldType.IsAssignableFrom(field.GetType()));
        }

        public void Weld(ContentField field) {
            field.ContentPart = this;
            _fields.Add(field);
        }
    }

    public class ContentPart<TRecord> : ContentPart {
        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }

}
