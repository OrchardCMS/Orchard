using System;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentManagement.Metadata {
    public class ContentPartInfo {
        public string PartName { get; set; }
        public Func<ContentTypePartDefinition, ContentPart> Factory { get; set; }
    }
}
