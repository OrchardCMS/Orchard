using System.Xml.Linq;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public class ExportContext {
        public XDocument Document { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }
}