using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class ExportActionContext {
        public ExportActionContext() {
            RecipeDocument = new XDocument();
            Files = new List<ExportedFileDescription>();
        }
        public XDocument RecipeDocument { get; set; }
        public IList<ExportedFileDescription> Files { get; set; }
    }
}