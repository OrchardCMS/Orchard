using System;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement {
    public class ContentField {
        public virtual ContentPart ContentPart { get; set; }
        public string Name { get { return PartFieldDefinition.Name; } }
        public IDictionary<string, string> Settings { get; private set; }

        public new ContentPartDefinition PartDefinition { get { return ContentPart.PartDefinition; } }
        public ContentPartDefinition.Field PartFieldDefinition { get; set; }
        public ContentFieldDefinition FieldDefinition { get { return PartFieldDefinition.FieldDefinition; } }

        public Func<string, string> Getter { get; set; }
        public Action<string, string> Setter { get; set; }
    }
}
