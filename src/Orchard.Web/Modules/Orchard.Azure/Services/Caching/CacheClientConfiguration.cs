using Microsoft.ApplicationServer.Caching;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Environment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;

namespace Orchard.Azure.Services.Caching {

    public class CacheClientConfiguration {

        public static CacheClientConfiguration FromPlatformConfiguration(ShellSettings shellSettings, string settingNamePrefix) {
            var portString = PlatformConfiguration.GetSetting(Constants.CachePortSettingName, shellSettings, settingNamePrefix);
            var isSharedCachingString = PlatformConfiguration.GetSetting(Constants.CacheIsSharedCachingSettingName, shellSettings, settingNamePrefix);
            return new CacheClientConfiguration() {
                HostIdentifier = PlatformConfiguration.GetSetting(Constants.CacheHostIdentifierSettingName, shellSettings, settingNamePrefix),
                CacheName = PlatformConfiguration.GetSetting(Constants.CacheCacheNameSettingName, shellSettings, settingNamePrefix),
                Hostname = PlatformConfiguration.GetSetting(Constants.CacheHostnameSettingName, shellSettings, settingNamePrefix),
                Port = String.IsNullOrWhiteSpace(portString) ? 0 : Int32.Parse(portString),
                AuthorizationToken = PlatformConfiguration.GetSetting(Constants.CacheAuthorizationTokenSettingName, shellSettings, settingNamePrefix),
                IsSharedCaching = String.IsNullOrWhiteSpace(isSharedCachingString) ? false : Boolean.Parse(isSharedCachingString)
            };
        }

        public string HostIdentifier {
            get;
            protected set;
        }

        public string CacheName {
            get;
            protected set;
        }

        public string Hostname {
            get;
            protected set;
        }

        public int Port {
            get;
            protected set;
        }

        public string AuthorizationToken {
            get;
            protected set;
        }

        public bool IsSharedCaching {
            get;
            protected set;
        }

        public bool CompressionIsEnabled {
            get;
            protected set;
        }

        public bool AutodiscoverIsEnabled {
            get {
                return String.IsNullOrWhiteSpace(Hostname) || Port == 0 || String.IsNullOrWhiteSpace(AuthorizationToken);
            }
        }

        public void Validate() {
            if (AutodiscoverIsEnabled) {
                if (String.IsNullOrWhiteSpace(HostIdentifier))
                    throw new Exception("AutoDiscover mode is detected but HostIdentifier is missing or empty.");
            }
        }

        public DataCache CreateCache() {
            var dataCacheFactoryConfiguration = new DataCacheFactoryConfiguration() {
                MaxConnectionsToServer = 32,
                UseLegacyProtocol = false,
                IsCompressionEnabled = CompressionIsEnabled
            };

            if (AutodiscoverIsEnabled)
                dataCacheFactoryConfiguration.AutoDiscoverProperty = new DataCacheAutoDiscoverProperty(true, HostIdentifier);
            else {
                dataCacheFactoryConfiguration.Servers = new[] { new DataCacheServerEndpoint(Hostname, Port) };
                dataCacheFactoryConfiguration.SecurityProperties = new DataCacheSecurity(AuthorizationToken);
            }

            var dataCacheFactory = new DataCacheFactory(dataCacheFactoryConfiguration);

            if (IsSharedCaching || String.IsNullOrEmpty(CacheName))
                return dataCacheFactory.GetDefaultCache();

            return dataCacheFactory.GetCache(CacheName);
        }

        public override string ToString() {
            string key = String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}", HostIdentifier, CacheName, Hostname, Port, AuthorizationToken, IsSharedCaching, CompressionIsEnabled);
            return key;
        }
    }
}