using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Mvc.ViewModels;

namespace Orchard.MultiTenancy.ViewModels {
    public class TenantsIndexViewModel : BaseViewModel {
        public IEnumerable<ShellSettings> TenantSettings { get; set; }
    }
}
