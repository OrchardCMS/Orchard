using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Orchard.Environment.Configuration {
    public class ExtensionLocations : IDependency {
        public string[] CoreLocations;
        public string[] ModuleLocations;
        public string[] ThemeLocations;
        public string[] CommonLocations;  // locations that should not be common and not related to the current tenant
        public string[] ModuleAndThemeLocations;
        public string[] ExtensionsVirtualPathPrefixes;  // Modules+Themes (no core)

        public ExtensionLocations() {
            Init(new DefaultAppConfigurationAccessor());
        }

        // This optional constructor can be used to create an environment that takes AppConfigurations from IAppConfigurationAccessor instead of from the global ConfigurationManager.AppSettings
        public ExtensionLocations(IAppConfigurationAccessor appConfigurationAccessor) {
            Init(appConfigurationAccessor);
        }

        public virtual void Init(IAppConfigurationAccessor appConfigurationAccessor) {
            CoreLocations = new string[] {"~/Core"};
            ModuleLocations = GetConfigPaths(appConfigurationAccessor, "Modules", "~/Modules");
            ThemeLocations = GetConfigPaths(appConfigurationAccessor, "Themes", "~/Themes" );
            CommonLocations = GetConfigPaths(appConfigurationAccessor, "Common", "~/Media")
                .Concat(ThemeLocations)
                .Concat(ModuleLocations)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            ModuleAndThemeLocations = ModuleLocations
                .Concat(ThemeLocations)
                .Distinct(StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            ExtensionsVirtualPathPrefixes = ModuleAndThemeLocations
                .Select(l=>l+"/")
                .OrderBy(l=>l.Count(c=>c=='/'))
                .Reverse()
                .ToArray();
        }

        /// <summary>
        /// Return module from path that is constructed as Location/Module/relative/path/in/module
        /// Path prefixes is expected as list of Location/ (location+trailing "/")
        /// 
        /// Extension locations can contain '/' so they are matched with deeper path first
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns>the module - or null of not found</returns>
        public static string ModuleMatch(string virtualPath, IEnumerable<string> pathPrefixes) {
            foreach(string prefix in pathPrefixes) {
                if(virtualPath.StartsWith(prefix)) {
                    int index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
                    if (index <= 0)
                        continue; 
                    var moduleName = virtualPath.Substring(prefix.Length, index - prefix.Length);
                    return (string.IsNullOrEmpty(moduleName) ? null : moduleName);
                }
            }
            return null;
        }

        /// <summary>
        /// Return module from path that is constructed as ExtensionLocation/Module/relative/path/in/module
        /// 
        /// Extension locations can contain '/' so they are matched with deeper path first
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns>the module - or null of not found</returns>
        public string ExtensionsModuleMatch(string virtualPath) {
            ModuleMatch(virtualPath, ExtensionsVirtualPathPrefixes);
            return null;
        }

        /// <summary>
        /// Return true if the virtual path starts with any of the prefixes
        /// </summary>
        public static bool PrefixMatch(string virtualPath, IEnumerable<string> pathPrefixes) {
            return pathPrefixes.Any(p => virtualPath.StartsWith(p));
        }

        public bool ExtensionsPrefixMatch(string virtualPath) {
            return PrefixMatch(virtualPath, ExtensionsVirtualPathPrefixes);
        }

        /// <summary>
        /// Get list of comma separated paths from web.config appSettings
        /// Also return the default path
        /// </summary>
        static string[] GetConfigPaths(IAppConfigurationAccessor appConfigurationAccessor, string key, string defaultPath) {
            char[] delim = { ',' };
            string configuration = appConfigurationAccessor.GetConfiguration(key) ?? "";
            return configuration.Split(delim, StringSplitOptions.RemoveEmptyEntries).Concat(new string[] { defaultPath }).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private class DefaultAppConfigurationAccessor : IAppConfigurationAccessor {
            public DefaultAppConfigurationAccessor() {}

            public string GetConfiguration(string name) {
                return ConfigurationManager.AppSettings[name];
            }
        }

    }
}