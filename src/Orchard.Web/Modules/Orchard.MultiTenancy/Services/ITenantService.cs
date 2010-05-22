using System.Collections.Generic;
using Orchard.Environment.Configuration;

namespace Orchard.MultiTenancy.Services {
    public interface ITenantService : IDependency {
        IEnumerable<ShellSettings> GetTenants();
        void CreateTenant(ShellSettings settings);
        void UpdateTenant(ShellSettings settings);
    }
}