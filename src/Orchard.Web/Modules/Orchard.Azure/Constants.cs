using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure {

    public class Constants {
        public const string ShellSettingsStorageConnectionStringSettingName = "Orchard.Settings.StorageConnectionString";
        public const string ShellSettingsContainerName = "sites"; // Container names must be lower case.
        public const string ShellSettingsFileName = "Settings.txt";

        public const string MediaStorageFeatureName = "Orchard.Azure";
        public const string MediaStorageStorageConnectionStringSettingName = "Orchard.Media.StorageConnectionString";
        public const string MediaStorageContainerName = "media"; // Container names must be lower case.

        public const string OutputCacheFeatureName = "Orchard.Azure.OutputCache";
        public const string OutputCacheHostIdentifierSettingName = "Azure.OutputCache.HostIdentifier";
        public const string OutputCacheCacheNameSettingName = "Azure.OutputCache.CacheName";
        public const string OutputCacheIsSharedCachingSettingName = "Azure.OutputCache.IsSharedCaching";

        public const string DatabaseCacheFeatureName = "Orchard.Azure.DatabaseCache";
        public const string DatabaseCacheHostIdentifierSettingName = "Azure.DatabaseCache.HostIdentifier";
        public const string DatabaseCacheCacheNameSettingName = "Azure.DatabaseCache.CacheName";
        public const string DatabaseCacheIsSharedCachingSettingName = "Azure.DatabaseCache.IsSharedCaching";
    }
}