using System;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData {
    public class ContentPartInfo {
        public string PartName { get; set; }
        public Func<ContentTypeDefinition.Part, ContentPart> Factory { get; set; }
    }
}
