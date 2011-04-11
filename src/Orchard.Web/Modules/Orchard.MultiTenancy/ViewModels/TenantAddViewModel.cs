using System.ComponentModel.DataAnnotations;
using Orchard.MultiTenancy.Annotations;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantAddViewModel  {
        public TenantAddViewModel() {
            // define "Allow the tenant to set up the database" as default value 
            DataProvider = "";
        }

        [Required]
        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }
        public string DataProvider { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
    }
}

