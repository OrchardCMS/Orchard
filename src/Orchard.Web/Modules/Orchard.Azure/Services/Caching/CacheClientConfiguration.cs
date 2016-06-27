using Microsoft.ApplicationServer.Caching;
using Orchard.Azure.Services.Environment.Configuration;
using System;

namespace Orchard.Azure.Services.Caching {

    public class CacheClientConfiguration {

        public static CacheClientConfiguration FromPlatformConfiguration(string tenant, string settingNamePrefix, IPlatformConfigurationAccessor pca) {
            return new CacheClientConfiguration {
                HostIdentifier = pca.GetSetting(Constants.CacheHostIdentifierSettingName, tenant, settingNamePrefix),
                CacheName = pca.GetSetting(Constants.CacheCacheNameSettingName, tenant, settingNamePrefix),
                AuthorizationToken = pca.GetSetting(Constants.CacheAuthorizationTokenSettingName, tenant, settingNamePrefix),
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

        public string AuthorizationToken {
            get;
            protected set;
        }

        public bool CompressionIsEnabled {
            get;
            set;
        }

        public void Validate() {
            if (String.IsNullOrWhiteSpace(HostIdentifier)) {
                throw new Exception("The HostIdentifier value is missing or empty.");
            }
        }

        public DataCacheFactory CreateCache() {
            var dataCacheFactoryConfiguration = new DataCacheFactoryConfiguration {
                UseLegacyProtocol = false,
                IsCompressionEnabled = CompressionIsEnabled
            };

            dataCacheFactoryConfiguration.AutoDiscoverProperty = new DataCacheAutoDiscoverProperty(true, HostIdentifier);
            if (!String.IsNullOrEmpty(AuthorizationToken))
                dataCacheFactoryConfiguration.SecurityProperties = new DataCacheSecurity(AuthorizationToken, sslEnabled: false);

            return new DataCacheFactory(dataCacheFactoryConfiguration);
            
            
        }

        public override int GetHashCode() {
            int hash = 37;
            hash = hash * 23 + (HostIdentifier != null ? HostIdentifier.GetHashCode() : 0);
            hash = hash * 23 + (CacheName != null ? CacheName.GetHashCode() : 0);
            hash = hash * 23 + (AuthorizationToken != null ? AuthorizationToken.GetHashCode() : 0);
            hash = hash * 23 + CompressionIsEnabled.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj) {
            var other = obj as CacheClientConfiguration;

            if (this == other) {
                return true;
            }

            return
                HostIdentifier == other.HostIdentifier &&
                CacheName == other.CacheName &&
                AuthorizationToken == other.AuthorizationToken &&
                CompressionIsEnabled == other.CompressionIsEnabled
                ;
        }
    }
}