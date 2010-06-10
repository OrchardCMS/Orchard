using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement {
    public abstract class ContentPart : IContent {
        public virtual ContentItem ContentItem { get; set; }
        public ContentTypeDefinition TypeDefinition { get { return ContentItem.TypeDefinition; } }
        public ContentTypeDefinition.Part TypePartDefinition { get; set; }
        public ContentPartDefinition PartDefinition { get { return TypePartDefinition.PartDefinition; } }

        public IEnumerable<ContentField> ContentFields { get; set; }
    }

    public class ContentPart<TRecord> : ContentPart {
        public readonly LazyField<TRecord> _record = new LazyField<TRecord>();
        public TRecord Record { get { return _record.Value; } set { _record.Value = value; } }
    }

}
