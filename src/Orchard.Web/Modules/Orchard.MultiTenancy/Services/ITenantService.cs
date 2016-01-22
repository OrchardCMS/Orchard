using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;

namespace Orchard.MultiTenancy.Services {
    public interface ITenantService : IDependency {
        /// <summary>
        /// Retrieves all tenants' shell settings.
        /// </summary>
        /// <returns>All tenants' shell settings.</returns>
        IEnumerable<ShellSettings> GetTenants();

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="settings">Shell settings of the tenant.</param>
        void CreateTenant(ShellSettings settings);

        /// <summary>
        /// Updates the shell settings of a tenant.
        /// </summary>
        /// <param name="settings">Shell settings of the tenant.</param>
        void UpdateTenant(ShellSettings settings);

        /// <summary>
        /// Returns a list of all installed themes.
        /// </summary>
        IEnumerable<ExtensionDescriptor> GetInstalledThemes();

        /// <summary>
        /// Returns a list of all installed modules
        /// </summary>
        IEnumerable<ExtensionDescriptor> GetInstalledModules();
    }
}