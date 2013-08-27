using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Azure.Services.FileSystems;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Logging;

namespace Orchard.Azure.Services.Environment.Configuration {

    /// <summary>
    /// Provides an IShellSettingsManager implementation that uses Windows Azure Blob Storage as the
    /// underlying storage system.
    /// </summary>
    /// <remarks>
    /// Additionally, this class handles role configuration change events when running in a Windows Azure Cloud
    /// Service to ensure all Orchard tenents are notified whenever a role configuration setting is changed
    /// through the management portal or API.
    /// </remarks>
    public class AzureBlobShellSettingsManager : Component, IShellSettingsManager {

        protected readonly IShellSettingsManagerEventHandler Events;
        protected readonly AzureFileSystem FileSystem;

        public AzureBlobShellSettingsManager(IShellSettingsManagerEventHandler events, IMimeTypeProvider mimeTypeProvider) {
            Events = events;
            FileSystem = new AzureFileSystem(CloudConfigurationManager.GetSetting(Constants.ShellSettingsStorageConnectionStringSettingName), Constants.ShellSettingsContainerName, String.Empty, true, mimeTypeProvider);
            if (RoleEnvironment.IsAvailable) {
                RoleEnvironment.Changing += RoleEnvironment_Changing;
                RoleEnvironment.Changed += RoleEnvironment_Changed;
            }
        }

        public virtual IEnumerable<ShellSettings> LoadSettings() {
            var settings = LoadSettingsInternal().ToArray();
            PlatformShellSettings.ApplyTo(settings); // Apply platform configuration overrides.
            return settings;
        }

        public virtual void SaveSettings(ShellSettings settings) {
            var content = ShellSettingsSerializer.ComposeSettings(settings);
            var filePath = FileSystem.Combine(settings.Name, Constants.ShellSettingsFileName);
            var file = FileSystem.FileExists(filePath) ? FileSystem.GetFile(filePath) : FileSystem.CreateFile(filePath);

            using (var stream = file.OpenWrite()) {
                using (var writer = new StreamWriter(stream)) {
                    writer.Write(content);
                }
            }

            Events.Saved(settings);
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

        private IEnumerable<ShellSettings> LoadSettingsInternal() {
            foreach (var folder in FileSystem.ListFolders(null)) {
                foreach (var file in FileSystem.ListFiles(folder.GetPath())) {
                    if (!String.Equals(file.GetName(), Constants.ShellSettingsFileName))
                        continue;
                    using (var stream = file.OpenRead()) {
                        using (var reader = new StreamReader(stream))
                            yield return ShellSettingsSerializer.ParseSettings(reader.ReadToEnd());
                    }
                }
            }
        }
    }
}
