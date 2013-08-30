using Microsoft.WindowsAzure;
using Orchard.Environment.Configuration;
using System;

namespace Orchard.Azure.Services.Environment.Configuration {

    public static class PlatformConfiguration {

        /// <summary>
        /// Reads a setting from platform configuration, trying first prefixed with the current tenant name and
        /// secondly with no prefix.
        /// </summary>
        /// <param name="name">The name of the setting to read.</param>
        /// <param name="shellSettings">The ShellSettings object for the current tenant.</param>
        /// <param name="namePrefix">An optional prefix to prepend the setting name with.</param>
        /// <returns>The value of the setting if found with or without tenant name prefix, otherwise null.</returns>
        public static string GetSetting(string name, ShellSettings shellSettings, string namePrefix = null) {
            var tenantName = shellSettings.Name + ":" + namePrefix + name;
            var fallbackName = namePrefix + name;
            return CloudConfigurationManager.GetSetting(tenantName) ?? CloudConfigurationManager.GetSetting(fallbackName);
        }
    }
}