using System.Collections.Generic;
using System.Xml.Linq;
using Orchard.Events;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    public class ExportContext {
        public XDocument Document { get; set; }
        public IEnumerable<string> ContentTypes { get; set; }
        public ExportOptions ExportOptions { get; set; }
    }

    public interface IExportEventHandler : IEventHandler {
        void Exporting(ExportContext context);
        void Exported(ExportContext context);
    }
}