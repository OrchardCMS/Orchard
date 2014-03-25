using Microsoft.WindowsAzure;
using System.Configuration;

namespace Orchard.Azure.Services.Environment.Configuration {

    public class DefaultPlatformConfigurationAccessor : IPlatformConfigurationAccessor {

        /// <summary>
        /// Trying to read a setting using the default implementation.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="tenant">The current tenant's name.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found in any of the available sources, otherwise null.</returns>
        /// <see cref="PlatformConfiguration" />
        public string GetSetting(string name, string tenant, string namePrefix = null) {
            return PlatformConfiguration.GetSetting(name, tenant, namePrefix);
        }
    }
}