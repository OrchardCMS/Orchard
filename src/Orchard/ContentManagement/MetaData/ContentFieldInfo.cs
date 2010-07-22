using System;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData {
    public class ContentFieldInfo {
        public string FieldTypeName { get; set; }
        public Func<ContentPartFieldDefinition, IFieldStorage, ContentField> Factory { get; set; }
    }
}
