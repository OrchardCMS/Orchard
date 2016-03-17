using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions.Models;

namespace Orchard.MultiTenancy.Services {
    public interface ITenantService : IDependency {
        /// <summary>
        /// Retrieves ShellSettings objects for all tenants.
        /// </summary>
        IEnumerable<ShellSettings> GetTenants();

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        /// <param name="settings">A ShellSettings object specifying the settings for the new tenant.</param>
        void CreateTenant(ShellSettings settings);

        /// <summary>
        /// Updates the settings of a tenant.
        /// </summary>
        /// <param name="settings">The new ShellSettings object for the tenant.</param>
        void UpdateTenant(ShellSettings settings);

        /// <summary>
        /// Resets a tenant to its uninitialized state.
        /// </summary>
        /// <param name="settings">A ShellSettings object to identify the tenant to reset.</param>
        /// <param name="dropDatabaseTables">A boolean indicated whether tenant database tables should be dropped also.</param>
        /// <param name="force">A boolean indicating whether reset should be performed even if the tenant state is <c>TenantState.Running</c>.</param>
        void ResetTenant(ShellSettings settings, bool dropDatabaseTables, bool force);

        /// <summary>
        /// Returns a list of all known database tables in a tenant.
        /// </summary>
        /// <param name="settings">A ShellSettings object to identify the tenant.</param>
        /// <returns>A list of known database table names for the tenant.</returns>
        IEnumerable<string> GetTenantDatabaseTableNames(ShellSettings settings);

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