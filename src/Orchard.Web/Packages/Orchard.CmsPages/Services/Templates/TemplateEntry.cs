using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Orchard.CmsPages.Services.Templates {
    public class TemplateEntry {
        public string Name { get; set; }
        public TextReader Content { get; set; }
    }
}
