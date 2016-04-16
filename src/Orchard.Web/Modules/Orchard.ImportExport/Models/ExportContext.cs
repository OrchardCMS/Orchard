using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ImportExport.Models {
    public class ExportContext {
        public XDocument Document { get; set; }
        [Obsolete]
        public ExportOptions ExportOptions { get; set; }

        public IList<ExportedFileDescription> Files { get; set; }
    }
}