using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement {
    public class ContentField : ContentPart {
        public virtual ContentPart ContentPart { get; set; }
        public string Name { get { return PartFieldDefinition.Name; } }
        public IDictionary<string, string> Settings { get; private set; }

        public new ContentPartDefinition PartDefinition { get { return ContentPart.PartDefinition; } }
        public ContentPartDefinition.Field PartFieldDefinition { get; set; }
        public ContentFieldDefinition FieldDefinition { get { return PartFieldDefinition.FieldDefinition; } }
    }

    public class ContentField<TRecord> : ContentField {
        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }
}
