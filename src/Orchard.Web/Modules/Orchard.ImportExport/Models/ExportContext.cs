using System;
using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class ExportContext {
        public XDocument Document { get; set; }
        [Obsolete]
        public ExportOptions ExportOptions { get; set; }
    }
}