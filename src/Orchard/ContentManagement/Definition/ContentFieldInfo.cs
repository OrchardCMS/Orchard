using System;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Definition.Models;

namespace Orchard.ContentManagement.Definition {
    public class ContentFieldInfo {
        public string FieldTypeName { get; set; }
        public Func<ContentPartFieldDefinition, IFieldStorage, ContentField> Factory { get; set; }
    }
}
