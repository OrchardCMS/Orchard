using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Orchard.Azure.Services.FileSystems;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.Services.Environment.Configuration {

    public class AzureBlobShellSettingsManager : Component, IShellSettingsManager {

        protected const string EmptyValueString = "null";
        protected readonly IShellSettingsManagerEventHandler Events;
        protected readonly AzureFileSystem FileSystem;

        public AzureBlobShellSettingsManager(IShellSettingsManagerEventHandler events, IMimeTypeProvider mimeTypeProvider) {
            Events = events;
            FileSystem = new AzureFileSystem(CloudConfigurationManager.GetSetting(Constants.ShellSettingsStorageConnectionStringSettingName), Constants.ShellSettingsContainerName, String.Empty, true, mimeTypeProvider);
        }

        public virtual IEnumerable<ShellSettings> LoadSettings() {
            var settings = LoadSettingsInternal().ToArray();
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
