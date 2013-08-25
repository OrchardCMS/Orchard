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
