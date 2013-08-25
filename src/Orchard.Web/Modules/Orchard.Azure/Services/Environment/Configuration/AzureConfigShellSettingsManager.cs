using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Logging;

namespace Orchard.Azure.Services.Environment.Configuration {

    /// <summary>
    /// Provides a ShellSettingsManager implementation that reads each value first
    /// from platform configuration and second from the Settings.txt file stored in 
    /// Azure Blob Storage (using AzureBlobShellSettingsManager).
    /// </summary>
    /// <remarks>
    /// This provider can be activated by configuration in Orchard.Web or Orchard.Azure.Web.
    /// 
    /// For each setting {SettingName} found in the Settings.txt file, this provider also looks
    /// for a corresponding platform setting Orchard.{TenantName}.{SettingName}. The
    /// <see cref="Microsoft.WindowsAzure.CloudConfigurationManager"/> class is used to search
    /// for platform configuration settings in Azure Cloud Service configuration settings, Azure
    /// Web Sites configuration settings and finally the Web.config appSettings element.
    /// 
    /// If a settings value is found, that value takes precedence over the Settings.txt
    /// file when settings are read. When settings are written, they are always only written
    /// to the backing Settings.txt file since cloud configuration settings are read-only.
    /// 
    /// This can be very useful in scenarios when you deploy your site to a number of different
    /// environments and want to use cloud configuration profiles to maintain separate
    /// settings for each of them that are automatically used when publishing. The fallback
    /// mechanism enables you to keep environment-specific read-only settings (such as
    /// SqlServerConnectionString) in cloud configuration profiles while leaving the
    /// remaining and writeable settings (such as State) in the Settings.txt file.
    /// 
    /// When distributing your settings across cloud configuration and Settings.txt, you
    /// must ensure that any settings that Orchard considers writeable (such as State) are
    /// not placed in cloud configuration.
    /// 
    /// This provider also handles role configuration change events when running in an
    /// Azure Cloud Service to ensure all Orchard tenents are notified whenever a role configuration
    /// settings is changed though the management portal or API.
    /// </remarks>
    public class AzureConfigShellSettingsManager : AzureBlobShellSettingsManager {

        private const string _prefix = "Orchard";

        public AzureConfigShellSettingsManager(IShellSettingsManagerEventHandler events, IMimeTypeProvider mimeTypeProvider)
            : base(events, mimeTypeProvider) {
            if (RoleEnvironment.IsAvailable) {
                RoleEnvironment.Changing += RoleEnvironment_Changing;
                RoleEnvironment.Changed += RoleEnvironment_Changed;
            }
        }

        public override IEnumerable<ShellSettings> LoadSettings() {     
            var settingsList = base.LoadSettings();
            
            foreach (var settings in settingsList) {
                foreach (var key in settings.Keys.ToArray()) {
                    var cloudConfigurationKey = String.Format("{0}.{1}.{2}", _prefix, settings.Name, key);
                    var cloudConfigurationValue = ParseValue(CloudConfigurationManager.GetSetting(cloudConfigurationKey));
                    if (cloudConfigurationValue != null)
                        settings[key] = cloudConfigurationValue;
                }
            }

            return settingsList;
        }

        void RoleEnvironment_Changing(object sender, RoleEnvironmentChangingEventArgs e) {
            // Indicate to the fabric controller that we can handle any changes.
            e.Cancel = false;
        }

        private void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e) {
            Logger.Debug("Handling RoleEnvironmentChanged event.");

            var configurationChangeQuery =
                from c in e.Changes
                where c is RoleEnvironmentConfigurationSettingChange
                select c;

            // If there's a configuration settings change, inform all Orchard tenants.
            if (configurationChangeQuery.Any()) {
                Logger.Information("Role configuration settings changed; refreshing Orchard shell settings.");
                var settingsList = LoadSettings();
                foreach (var settings in settingsList)
                    Events.Saved(settings);
            }
        }

        private string ParseValue(string value) {
            if (value == EmptyValueString || String.IsNullOrWhiteSpace(value))
                return null;
            return value;
        }
    }
}
