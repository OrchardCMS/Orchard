using System.ComponentModel.DataAnnotations;
using Orchard.MultiTenancy.Annotations;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantAddViewModel  {
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

