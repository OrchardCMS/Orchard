using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement {
    public class ContentField {
        public string Name { get; set; }
        public ContentFieldDefinition Definition { get; set; }
    }
}
