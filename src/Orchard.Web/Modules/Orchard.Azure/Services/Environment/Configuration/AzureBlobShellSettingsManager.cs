using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Azure.Services.FileSystems;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;
using Orchard.Logging;
using Microsoft.Azure;

namespace Orchard.Azure.Services.Environment.Configuration {

    /// <summary>
    /// Provides an IShellSettingsManager implementation that uses Microsoft Azure Blob Storage as the
    /// underlying storage system.
    /// </summary>
    public class AzureBlobShellSettingsManager : IShellSettingsManager {

        private readonly AzureFileSystem _fileSystem;
        private readonly IShellSettingsManagerEventHandler _events;

        public AzureBlobShellSettingsManager(IMimeTypeProvider mimeTypeProvider, IShellSettingsManagerEventHandler events) {
            _fileSystem = new AzureFileSystem(CloudConfigurationManager.GetSetting(Constants.ShellSettingsStorageConnectionStringSettingName), Constants.ShellSettingsContainerName, String.Empty, true, mimeTypeProvider);
            _events = events;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public virtual IEnumerable<ShellSettings> LoadSettings() {
            Logger.Debug("Reading ShellSettings...");
            var settingsList = LoadSettingsInternal().ToArray();

            var tenantNamesQuery =
                from settings in settingsList
                select settings.Name;
            Logger.Debug("Returning {0} ShellSettings objects for tenants {1}.", tenantNamesQuery.Count(), String.Join(", ", tenantNamesQuery));

            return settingsList;
        }

        public virtual void SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (String.IsNullOrEmpty(settings.Name))
                throw new ArgumentException("The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.", "settings");

            Logger.Debug("Saving ShellSettings for tenant '{0}'...", settings.Name);
            var content = ShellSettingsSerializer.ComposeSettings(settings);
            var filePath = _fileSystem.Combine(settings.Name, Constants.ShellSettingsFileName);
            var file = _fileSystem.FileExists(filePath) ? _fileSystem.GetFile(filePath) : _fileSystem.CreateFile(filePath);

            using (var stream = file.OpenWrite()) {
                using (var writer = new StreamWriter(stream)) {
                    writer.Write(content);
                }
            }

            Logger.Debug("ShellSettings saved successfully; flagging tenant '{0}' for restart.", settings.Name);
            _events.Saved(settings);
        }

        private IEnumerable<ShellSettings> LoadSettingsInternal() {
            foreach (var folder in _fileSystem.ListFolders(null)) {
                foreach (var file in _fileSystem.ListFiles(folder.GetPath())) {
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
