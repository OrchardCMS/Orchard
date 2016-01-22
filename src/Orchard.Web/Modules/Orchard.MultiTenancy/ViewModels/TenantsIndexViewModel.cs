using System.Collections.Generic;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantsIndexViewModel  {
        public IEnumerable<ShellSettings> TenantSettings { get; set; }
    }
}
