using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orchard.Environment.Configuration {
    public class AppConfigurationAccessor : IAppConfigurationAccessor {
        public string GetConfiguration(string name) {
            var appSettingsValue = ConfigurationManager.AppSettings[name];
            if (appSettingsValue != null) {
                return appSettingsValue;
            }

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[name];
            if (connectionStringSettings != null) {
                return connectionStringSettings.ConnectionString;
            }

            return String.Empty;
        }
    }
}
