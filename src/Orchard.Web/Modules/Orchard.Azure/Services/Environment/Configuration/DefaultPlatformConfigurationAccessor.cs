using Microsoft.WindowsAzure;
using System.Configuration;

namespace Orchard.Azure.Services.Environment.Configuration {

    public class DefaultPlatformConfigurationAccessor : IPlatformConfigurationAccessor {

        /// <summary>
        /// Trying to read a setting from the following sources in the following order (with and then without tenant name prefix):
        /// CloudConfigurationManager, ConnectionStrings, AppSettings.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="tenant">The current tenant's name.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found in any of the available sources, otherwise null.</returns>
        public string GetSetting(string name, string tenant, string namePrefix = null) {
            var tenantName = tenant + ":" + (namePrefix ?? string.Empty) + name;
            var fallbackName = (namePrefix ?? string.Empty) + name;

            var settingFromCloudConfiguration = CloudConfigurationManager.GetSetting(tenantName) ?? CloudConfigurationManager.GetSetting(fallbackName);
            if (!string.IsNullOrEmpty(settingFromCloudConfiguration)) return settingFromCloudConfiguration;

            var settingFromConnectionStrings = ConfigurationManager.ConnectionStrings[tenantName] ?? ConfigurationManager.ConnectionStrings[fallbackName];
            if (settingFromConnectionStrings != null) return settingFromConnectionStrings.ConnectionString;

            return ConfigurationManager.AppSettings[tenantName] ?? ConfigurationManager.AppSettings[fallbackName];
        }
    }
}