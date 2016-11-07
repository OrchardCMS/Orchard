namespace Orchard.Azure {

    public class Constants {
        public const string ShellSettingsStorageConnectionStringSettingName = "Orchard.Azure.Settings.StorageConnectionString";
        public const string ShellSettingsContainerNameSettingName = "Orchard.Azure.Settings.ContainerName";
        public const string ShellSettingsDefaultContainerName = "sites"; // Container names must be lower case.
        public const string ShellSettingsFileName = "Settings.txt";

        public const string MediaStorageFeatureName = "Orchard.Azure.Media";
        public const string MediaStorageStorageConnectionStringSettingName = "Orchard.Azure.Media.StorageConnectionString";
        public const string MediaStorageRootFolderPathSettingName = "Orchard.Azure.Media.RootFolderPath";
        public const string MediaStorageContainerNameSettingName = "Orchard.Azure.Media.ContainerName";
        public const string MediaStorageDefaultContainerName = "media"; // Container names must be lower case.
        public const string MediaStoragePublicHostName = "Orchard.Azure.Media.StoragePublicHostName";

        public const string OutputCacheFeatureName = "Orchard.Azure.OutputCache";
        public const string OutputCacheSettingNamePrefix = "Orchard.Azure.OutputCache.";
        public const string DatabaseCacheFeatureName = "Orchard.Azure.DatabaseCache";
        public const string DatabaseCacheSettingNamePrefix = "Orchard.Azure.DatabaseCache.";

        public const string CacheHostIdentifierSettingName = "HostIdentifier";
        public const string CacheCacheNameSettingName = "CacheName";
        public const string CacheAuthorizationTokenSettingName = "AuthorizationToken";
    }
}