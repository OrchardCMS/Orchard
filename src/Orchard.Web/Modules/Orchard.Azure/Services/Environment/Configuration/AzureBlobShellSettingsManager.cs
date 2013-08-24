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

        public const string ConnectionStringSettingName = "Orchard.StorageConnectionString";
        public const string ContainerName = "sites"; // Container names must be lower cased.
        public const string SettingsFilename = "Settings.txt";
        public const char Separator = ':';
        public const string EmptyValue = "null";

        protected readonly IShellSettingsManagerEventHandler Events;
        protected readonly AzureFileSystem FileSystem;

        public AzureBlobShellSettingsManager(IShellSettingsManagerEventHandler events, IMimeTypeProvider mimeTypeProvider)
            : this(CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(ConnectionStringSettingName)), events, mimeTypeProvider) {
        }

        public AzureBlobShellSettingsManager(CloudStorageAccount storageAccount, IShellSettingsManagerEventHandler events, IMimeTypeProvider mimeTypeProvider) {
            Events = events;
            FileSystem = new AzureFileSystem(ContainerName, String.Empty, true, mimeTypeProvider);
        }

        public virtual IEnumerable<ShellSettings> LoadSettings() {
            var settings = LoadSettingsInternal().ToArray();
            return settings;
        }

        public virtual void SaveSettings(ShellSettings settings) {
            var content = ShellSettingsSerializer.ComposeSettings(settings);
            var filePath = FileSystem.Combine(settings.Name, SettingsFilename);

            var file = FileSystem.FileExists(filePath)
                ? FileSystem.GetFile(filePath)
                : FileSystem.CreateFile(filePath);

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
                    if (!String.Equals(file.GetName(), SettingsFilename))
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
