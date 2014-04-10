using Microsoft.WindowsAzure;
using System.Configuration;
using System;

namespace Orchard.Azure.Services.Environment.Configuration {

    /// <summary>
    /// Provides a default <c>IPlatformConfigurationAccessor</c> implementation that reads configuration settings
    /// from cloud service role configuration, app settings and connection strings.
    /// </summary>
    /// <remarks>
    /// Settings are read first using the <c>CloudConfigurationManager</c> class, which looks first in cloud service role
    /// configuration if running in a Microsoft Azure Cloud Service and secondly in app settings (either in Web.config or in
    /// Microsoft Azure Web Site app settings configuration). If the setting is not found using CloudConfigurationManager
    /// then connection strings (either in Web.config or in Microsoft Azure Web Site connection strings configuration) is
    /// checked. Both the tenant-specific name and the tenant-neutral name are checked within each configuration source
    /// before proceeding to the next one.
    /// </remarks>
    public class DefaultPlatformConfigurationAccessor : IPlatformConfigurationAccessor {

        public string GetSetting(string name, string tenant, string namePrefix = null) {
            var tenantName = String.Format("{0}:{1}{2}", tenant, namePrefix, name);
            var fallbackName = String.Format("{0}{1}", namePrefix, name);

            var cloudConfigurationValue = CloudConfigurationManager.GetSetting(tenantName) ?? CloudConfigurationManager.GetSetting(fallbackName);
            if (!String.IsNullOrEmpty(cloudConfigurationValue))
                return cloudConfigurationValue;

            var connectionStringValue = ConfigurationManager.ConnectionStrings[tenantName] ?? ConfigurationManager.ConnectionStrings[fallbackName];
            if (connectionStringValue != null)
                return connectionStringValue.ConnectionString;

            return null;
        }
    }
}