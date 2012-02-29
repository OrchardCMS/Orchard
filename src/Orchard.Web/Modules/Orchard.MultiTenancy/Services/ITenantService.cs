using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;

namespace Orchard.MultiTenancy.Services {
    public interface ITenantService : IDependency {
        IEnumerable<ShellSettings> GetTenants();
        void CreateTenant(ShellSettings settings);
        void UpdateTenant(ShellSettings settings);

        /// <summary>
        /// Returns a list of all installed themes
        /// </summary>
        IEnumerable<ExtensionDescriptor> GetInstalledThemes();
    }
}