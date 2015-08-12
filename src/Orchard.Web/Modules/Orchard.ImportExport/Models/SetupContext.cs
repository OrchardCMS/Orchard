using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.ImportExport.Models {
    public class SetupContext {
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string DatabaseProvider { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public IEnumerable<string> EnabledFeatures { get; set; }
        public XDocument RecipeDocument { get; set; }
    }
}