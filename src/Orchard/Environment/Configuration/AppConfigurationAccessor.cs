using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Environment.Configuration {
    public class AppConfigurationAccessor : IAppConfigurationAccessor {
        private readonly ShellSettings _shellSettings;
        public AppConfigurationAccessor(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public string GetConfiguration(string name) {
            var tenantName = String.Format("{0}:{1}", _shellSettings.Name, name);

            var appSettingsValue = ConfigurationManager.AppSettings[tenantName] ?? ConfigurationManager.AppSettings[name];
            if (appSettingsValue != null) {
                return appSettingsValue;
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[tenantName] ?? ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings != null) {
                return connectionStringSettings.ConnectionString;
            }

            return String.Empty;
        }
    }
}
