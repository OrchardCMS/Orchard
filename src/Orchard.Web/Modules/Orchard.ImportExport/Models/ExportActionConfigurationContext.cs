using System.Xml.Linq;
using Orchard.Recipes.Models;

namespace Orchard.ImportExport.Models {
    public class ExportActionConfigurationContext : ConfigurationContext {
        public ExportActionConfigurationContext(XElement configurationElement) : base(configurationElement) {
        }
    }
}