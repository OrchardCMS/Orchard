using System.Collections.Generic;

namespace Orchard.ImportExport.Services {
    public class SetupContext {
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string DatabaseProvider { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public IEnumerable<string> EnabledFeatures { get; set; }
        public string RecipeText { get; set; }
        public bool DropExistingTables { get; set; }
    }
}