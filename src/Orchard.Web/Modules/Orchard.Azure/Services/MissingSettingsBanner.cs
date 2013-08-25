using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.UI.Admin.Notification;
using Orchard.UI.Notify;
using Orchard.Environment.Features;
using Orchard.Environment.Configuration;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace Orchard.Azure.Services {

    public class MissingSettingsBanner : INotificationProvider {

        public MissingSettingsBanner(IFeatureManager featureManager, ShellSettings shellSettings) {
            _featureManager = featureManager;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
        }

        private IFeatureManager _featureManager;
        private ShellSettings _shellSettings;

        public Localizer T {
            get;
            set;
        }

        public IEnumerable<NotifyEntry> GetNotifications() {
            var enabledFeatureNamesQuery =
                from feature in _featureManager.GetEnabledFeatures()
                select feature.Id;
            
            if (enabledFeatureNamesQuery.Contains(Constants.MediaStorageFeatureName)) {
                CloudStorageAccount storageAccount = null;
                var storageConnectionString = CloudConfigurationManager.GetSetting(Constants.MediaStorageStorageConnectionStringSettingName);
                var isValid = CloudStorageAccount.TryParse(storageConnectionString, out storageAccount);
                if (!isValid)
                    yield return new NotifyEntry {
                        Message = T("A valid storage account connection string must be configured in the role configuration setting '{0}'.", Constants.MediaStorageStorageConnectionStringSettingName),
                        Type = NotifyType.Warning
                    };
            }

            if (enabledFeatureNamesQuery.Contains(Constants.OutputCacheFeatureName)) {
                // Create default configuration to local role-based cache when feature is enabled.
                if (!_shellSettings.Keys.Contains(Constants.OutputCacheHostIdentifierSettingName))
                    _shellSettings[Constants.OutputCacheHostIdentifierSettingName] = "Orchard.Azure.Web";
                if (!_shellSettings.Keys.Contains(Constants.OutputCacheCacheNameSettingName))
                    _shellSettings[Constants.OutputCacheCacheNameSettingName] = "OutputCache";
                if (!_shellSettings.Keys.Contains(Constants.OutputCacheIsSharedCachingSettingName))
                    _shellSettings[Constants.OutputCacheIsSharedCachingSettingName] = "false";

                if (String.IsNullOrWhiteSpace(_shellSettings[Constants.OutputCacheHostIdentifierSettingName]))
                    yield return new NotifyEntry {
                        Message = T("A cache cluster host identifier must be configured in the shell setting '{0}'.", Constants.OutputCacheHostIdentifierSettingName),
                        Type = NotifyType.Warning
                    };
                if (String.IsNullOrWhiteSpace(_shellSettings[Constants.OutputCacheCacheNameSettingName]))
                    yield return new NotifyEntry {
                        Message = T("A cache name must be configured in the shell setting '{0}'.", Constants.OutputCacheCacheNameSettingName),
                        Type = NotifyType.Warning
                    };
                bool result;
                if (!Boolean.TryParse(_shellSettings[Constants.OutputCacheIsSharedCachingSettingName], out result))
                    yield return new NotifyEntry {
                        Message = T("A valid boolean value must be configured in the shell setting '{0}'.", Constants.OutputCacheIsSharedCachingSettingName),
                        Type = NotifyType.Warning
                    };
            }

            if (enabledFeatureNamesQuery.Contains(Constants.DatabaseCacheFeatureName)) {
                // Create default configuration to local role-based cache when feature is enabled.
                if (!_shellSettings.Keys.Contains(Constants.DatabaseCacheHostIdentifierSettingName))
                    _shellSettings[Constants.DatabaseCacheHostIdentifierSettingName] = "Orchard.Azure.Web";
                if (!_shellSettings.Keys.Contains(Constants.DatabaseCacheCacheNameSettingName))
                    _shellSettings[Constants.DatabaseCacheCacheNameSettingName] = "DatabaseCache";
                if (!_shellSettings.Keys.Contains(Constants.DatabaseCacheIsSharedCachingSettingName))
                    _shellSettings[Constants.DatabaseCacheIsSharedCachingSettingName] = "false";

                if (String.IsNullOrWhiteSpace(_shellSettings[Constants.DatabaseCacheHostIdentifierSettingName]))
                    yield return new NotifyEntry {
                        Message = T("A cache cluster host identifier must be configured in the shell setting '{0}'.", Constants.DatabaseCacheHostIdentifierSettingName),
                        Type = NotifyType.Warning
                    };
                if (String.IsNullOrWhiteSpace(_shellSettings[Constants.DatabaseCacheCacheNameSettingName]))
                    yield return new NotifyEntry {
                        Message = T("A cache name must be configured in the shell setting '{0}'.", Constants.DatabaseCacheCacheNameSettingName),
                        Type = NotifyType.Warning
                    };
                bool result;
                if (!Boolean.TryParse(_shellSettings[Constants.DatabaseCacheIsSharedCachingSettingName], out result))
                    yield return new NotifyEntry {
                        Message = T("A valid boolean value must be configured in the shell setting '{0}'.", Constants.DatabaseCacheIsSharedCachingSettingName),
                        Type = NotifyType.Warning
                    };
            }
        }
    }
}