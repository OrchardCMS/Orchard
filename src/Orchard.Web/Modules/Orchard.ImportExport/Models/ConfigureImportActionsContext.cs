using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class ConfigureImportActionsContext {

        public ConfigureImportActionsContext(XDocument configurationDocument) {
            ConfigurationDocument = configurationDocument;
        }
        
        public XDocument ConfigurationDocument { get; set; }
    }
}