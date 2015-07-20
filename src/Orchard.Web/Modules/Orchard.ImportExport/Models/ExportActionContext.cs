using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class ExportActionContext {
        public ExportActionContext() {
            RecipeDocument = new XDocument();
        }
        public XDocument RecipeDocument { get; set; }
    }
}