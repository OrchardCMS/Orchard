using System;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentManagement.Metadata {
    public class ContentFieldInfo {
        public string FieldTypeName { get; set; }
        public Func<ContentPartFieldDefinition, IFieldStorage, ContentField> Factory { get; set; }
    }
}
