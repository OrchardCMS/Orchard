using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class ImportActionContext {
        public XDocument RecipeDocument { get; set; }
        public string ExecutionId { get; set; }
    }
}