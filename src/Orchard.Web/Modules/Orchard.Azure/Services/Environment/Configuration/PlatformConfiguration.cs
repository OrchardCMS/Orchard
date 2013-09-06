using Microsoft.WindowsAzure;

namespace Orchard.Azure.Services.Environment.Configuration {

    public static class PlatformConfiguration {

        /// <summary>
        /// Reads a setting from platform configuration, trying first prefixed with the current tenant name and
        /// secondly with no prefix.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="tenant">The curren tenant's name.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found with or without tenant name prefix, otherwise null.</returns>
        public static string GetSetting(string name, string tenant, string namePrefix = null) {
            var tenantName = tenant + ":" + (namePrefix ?? string.Empty) + name;
            var fallbackName = (namePrefix ?? string.Empty) + name;
            return CloudConfigurationManager.GetSetting(tenantName) ?? CloudConfigurationManager.GetSetting(fallbackName);
        }
    }
}