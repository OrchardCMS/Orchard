using System;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData {
    public class ContentFieldInfo {
        public string FieldTypeName { get; set; }
        public Func<ContentPartDefinition.Field, ContentField> Factory { get; set; }
    }
}
