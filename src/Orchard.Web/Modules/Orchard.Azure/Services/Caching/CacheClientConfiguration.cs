using Microsoft.ApplicationServer.Caching;
using Orchard.Azure.Services.Environment.Configuration;
using System;

namespace Orchard.Azure.Services.Caching {

    public class CacheClientConfiguration {

        public static CacheClientConfiguration FromPlatformConfiguration(string settingNamePrefix) {
            var portString = PlatformConfiguration.GetSetting(Constants.CachePortSettingName, settingNamePrefix);
            var isSharedCachingString = PlatformConfiguration.GetSetting(Constants.CacheIsSharedCachingSettingName, settingNamePrefix);
            return new CacheClientConfiguration {
                HostIdentifier = PlatformConfiguration.GetSetting(Constants.CacheHostIdentifierSettingName, settingNamePrefix),
                CacheName = PlatformConfiguration.GetSetting(Constants.CacheCacheNameSettingName, settingNamePrefix),
                Hostname = PlatformConfiguration.GetSetting(Constants.CacheHostnameSettingName, settingNamePrefix),
                Port = String.IsNullOrWhiteSpace(portString) ? 0 : Int32.Parse(portString),
                AuthorizationToken = PlatformConfiguration.GetSetting(Constants.CacheAuthorizationTokenSettingName, settingNamePrefix),
                IsSharedCaching = !String.IsNullOrWhiteSpace(isSharedCachingString) && Boolean.Parse(isSharedCachingString)
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
            if (AutodiscoverIsEnabled && String.IsNullOrWhiteSpace(HostIdentifier)) {
                throw new Exception("AutoDiscover mode is detected but HostIdentifier is missing or empty.");
            }
        }

        public DataCache CreateCache() {
            var dataCacheFactoryConfiguration = new DataCacheFactoryConfiguration {
                MaxConnectionsToServer = 32,
                UseLegacyProtocol = false,
                IsCompressionEnabled = CompressionIsEnabled
            };

            if (AutodiscoverIsEnabled) {
                dataCacheFactoryConfiguration.AutoDiscoverProperty = new DataCacheAutoDiscoverProperty(true, HostIdentifier);
            }
            else {
                dataCacheFactoryConfiguration.Servers = new[] {new DataCacheServerEndpoint(Hostname, Port)};
                dataCacheFactoryConfiguration.SecurityProperties = new DataCacheSecurity(AuthorizationToken);
            }

            var dataCacheFactory = new DataCacheFactory(dataCacheFactoryConfiguration);

            if (IsSharedCaching || String.IsNullOrEmpty(CacheName)) {
                return dataCacheFactory.GetDefaultCache();
            }

            return dataCacheFactory.GetCache(CacheName);
        }

        public override string ToString() {
            var key = HostIdentifier + "_" + CacheName + "_" + Hostname + "_" + Port + "_" + AuthorizationToken + "_" + IsSharedCaching + "_" + CompressionIsEnabled;
            return key;
        }
    }
}