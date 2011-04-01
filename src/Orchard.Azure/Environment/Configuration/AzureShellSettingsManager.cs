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
            var content = ComposeSettings(settings);
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
                        yield return ParseSettings(reader.ReadToEnd());
                }
        }

        static ShellSettings ParseSettings(string text) {
            var shellSettings = new ShellSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;

            var settings = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var setting in settings) {
                var separatorIndex = setting.IndexOf(Separator);
                if(separatorIndex == -1) {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (value != EmptyValue) {
                    switch (key) {
                        case "Name":
                            shellSettings.Name = value;
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = value;
                            break;
                        case "State":
                            shellSettings.State = new TenantState(value);
                            break;
                        case "DataConnectionString":
                            shellSettings.DataConnectionString = value;
                            break;
                        case "DataPrefix":
                            shellSettings.DataTablePrefix = value;
                            break;
                        case "RequestUrlHost":
                            shellSettings.RequestUrlHost = value;
                            break;
                        case "RequestUrlPrefix":
                            shellSettings.RequestUrlPrefix = value;
                            break;
                        case "EncryptionAlgorithm":
                            shellSettings.EncryptionAlgorithm = value;
                            break;
                        case "EncryptionKey":
                            shellSettings.EncryptionKey = value;
                            break;
                        case "HashAlgorithm":
                            shellSettings.HashAlgorithm = value;
                            break;
                        case "HashKey":
                            shellSettings.HashKey = value;
                            break;
                    }
                }
            }

            return shellSettings;
        }

        static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            return string.Format("Name: {0}\r\nDataProvider: {1}\r\nDataConnectionString: {2}\r\nDataPrefix: {3}\r\nRequestUrlHost: {4}\r\nRequestUrlPrefix: {5}\r\nState: {6}\r\nEncryptionAlgorithm: {7}\r\nEncryptionKey: {8}\r\nHashAlgorithm: {9}\r\nHashKey: {10}\r\n",
                                 settings.Name,
                                 settings.DataProvider,
                                 settings.DataConnectionString ?? EmptyValue,
                                 settings.DataTablePrefix ?? EmptyValue,
                                 settings.RequestUrlHost ?? EmptyValue,
                                 settings.RequestUrlPrefix ?? EmptyValue,
                                 settings.State != null ? settings.State.ToString() : String.Empty,
                                 settings.EncryptionAlgorithm ?? EmptyValue,
                                 settings.EncryptionKey ?? EmptyValue,
                                 settings.HashAlgorithm ?? EmptyValue,
                                 settings.HashKey ?? EmptyValue
                );
        }
    }
}
