using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.ImportExport.Models {
    public class ImportActionConfigurationContext : ConfigurationContext {
        public ImportActionConfigurationContext(XElement configurationElement) : base(configurationElement) {
        }
    }
}