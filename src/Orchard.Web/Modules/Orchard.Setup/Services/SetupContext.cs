using System.Collections.Generic;
using Orchard.Recipes.Models;

namespace Orchard.Setup.Services {
    public class SetupContext {
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string DatabaseProvider { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public IEnumerable<string> EnabledFeatures { get; set; }
        public Recipe Recipe { get; set; }
    }
}