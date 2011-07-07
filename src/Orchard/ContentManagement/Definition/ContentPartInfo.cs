using System;
using Orchard.ContentManagement.Definition.Models;

namespace Orchard.ContentManagement.Definition {
    public class ContentPartInfo {
        public string PartName { get; set; }
        public Func<ContentTypePartDefinition, ContentPart> Factory { get; set; }
    }
}
