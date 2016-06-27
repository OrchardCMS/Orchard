using System.ComponentModel.DataAnnotations;
using Orchard.Environment.Configuration;
using Orchard.MultiTenancy.Annotations;
using System.Collections.Generic;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantEditViewModel  {
        public TenantEditViewModel() {
            Themes = new List<ThemeEntry>();
            Modules = new List<ModuleEntry>();
        }

        [Required]
        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }
        public string DataProvider { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public TenantState State { get; set; }

        public List<ThemeEntry> Themes { get; set; }
        public List<ModuleEntry> Modules { get; set; }

    }
}

