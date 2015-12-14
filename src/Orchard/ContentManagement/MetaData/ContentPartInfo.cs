using System;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData {
    public class ContentPartInfo {
        public string PartName { get; set; }
        public Func<ContentTypePartDefinition, ContentPart> Factory { get; set; }
    }
}
