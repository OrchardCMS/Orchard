using System.ComponentModel.DataAnnotations;
using Orchard.Environment.Configuration;
using Orchard.MultiTenancy.Annotations;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantEditViewModel  {
        [Required]
        public string Name { get; set; }
        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }
        public string DataProvider { get; set; }
        [SqlDatabaseConnectionString]
        public string DatabaseConnectionString { get; set; }
        public string DatabaseTablePrefix { get; set; }
        public TenantState State { get; set; }
    }
}

