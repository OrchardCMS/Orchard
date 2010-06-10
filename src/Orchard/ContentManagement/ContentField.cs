using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement {
    public class ContentField : IContent {
        public virtual ContentItem ContentItem { get; set;}
        public string Name { get; set; }
        public ContentFieldDefinition Definition { get; set; }
        public IDictionary<string, string> Settings { get; private set; }
    }

    public class ContentField<TRecord> : ContentField {
        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }
}
