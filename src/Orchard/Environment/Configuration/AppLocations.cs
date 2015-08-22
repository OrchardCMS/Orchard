using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Orchard.Environment.Configuration {
    public class AppLocations : IDependency {
        public string[] CoreLocations;
        public string[] ModuleLocations;
        public string[] ThemeLocations;
        public string[] CommonLocations;  // locations that should not be common and not related to the current tenant

        public AppLocations(IAppConfigurationAccessor appConfigurationAccessor)
        {
            Init(appConfigurationAccessor ?? new DefaultAppConfigurationAccessor() );
        }

        public void Init(IAppConfigurationAccessor appConfigurationAccessor)
        {
            CoreLocations = new string[] {"~/Core"};
            ModuleLocations = GetConfigPaths(appConfigurationAccessor, "Modules", "~/Modules");
            ThemeLocations = GetConfigPaths(appConfigurationAccessor, "Themes", "~/Themes" );
            CommonLocations = GetConfigPaths(appConfigurationAccessor, "Common", "~/Media")
                .Concat(ThemeLocations)
                .Concat(ModuleLocations)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        /// <summary>
        /// Get list of comma separated paths from web.config appSettings
        /// Also return the default path
        /// </summary>
        static string[] GetConfigPaths(IAppConfigurationAccessor appConfigurationAccessor, string key, string defaultPath)
        {
            char[] delim = new char[] { ',' };
            string configuration = appConfigurationAccessor.GetConfiguration(key) ?? "";
            return configuration.Split(delim, StringSplitOptions.RemoveEmptyEntries).Concat(new string[] { defaultPath }).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private class DefaultAppConfigurationAccessor : IAppConfigurationAccessor
        {
            public DefaultAppConfigurationAccessor()
            {
            }

            public string GetConfiguration(string name)
            {
                return ConfigurationManager.AppSettings[name];
            }
        }


    }
}