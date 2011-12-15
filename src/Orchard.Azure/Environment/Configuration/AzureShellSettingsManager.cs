using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Environment.Configuration;

namespace Orchard.Azure.Environment.Configuration {

    public class AzureShellSettingsManager : IShellSettingsManager {
        public const string ContainerName = "sites"; // container names must be lower cased
        public const string SettingsFilename = "Settings.txt";
        public const char Separator = ':';
        public const string EmptyValue = "null";

        private readonly IShellSettingsManagerEventHandler _events;
        private readonly AzureFileSystem _fileSystem;

        public AzureShellSettingsManager(IShellSettingsManagerEventHandler events)
            : this(CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString")), events) {}

        public AzureShellSettingsManager(CloudStorageAccount storageAccount, IShellSettingsManagerEventHandler events) {
            _events = events;
            _fileSystem = new AzureFileSystem(ContainerName, String.Empty, true, storageAccount);
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var settings = LoadSettings().ToArray();
            return settings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings) {
            var content = ShellSettingsSerializer.ComposeSettings(settings);
            var filePath = _fileSystem.Combine(settings.Name, SettingsFilename);

            var file = _fileSystem.FileExists(filePath) 
                ? _fileSystem.GetFile(filePath)
                : _fileSystem.CreateFile(filePath);

            using (var stream = file.OpenWrite()) {
                using (var writer = new StreamWriter(stream)) {
                    writer.Write(content);
                }
            }

            _events.Saved(settings);
        }

        IEnumerable<ShellSettings> LoadSettings() {
            foreach (var folder in _fileSystem.ListFolders(null))
                foreach (var file in _fileSystem.ListFiles(folder.GetPath())) {
                    if (!String.Equals(file.GetName(), SettingsFilename))
                        continue;

                    using (var stream = file.OpenRead())
                    using (var reader = new StreamReader(stream))
                        yield return ShellSettingsSerializer.ParseSettings(reader.ReadToEnd());
                }
        }
    }
}
