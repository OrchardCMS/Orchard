using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.Azure.Environment.Configuration {

    public class AzureShellSettingsManager : IShellSettingsManager {
        public const string ContainerName = "sites"; // container names must be lower cased
        public const string SettingsFilename = "Settings.txt";

        private readonly IShellSettingsManagerEventHandler _events;
        private readonly AzureFileSystem _fileSystem;

        Localizer T { get; set; }

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
            var filePath = String.Concat(settings.Name, "/", SettingsFilename);
            var file = _fileSystem.CreateFile(filePath);

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

            string[] settings = text.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var setting in settings) {
                string[] settingFields = setting.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
                int fieldsLength = settingFields.Length;
                if (fieldsLength != 2)
                    continue;
                for (int i = 0; i < fieldsLength; i++) {
                    settingFields[i] = settingFields[i].Trim();
                }
                if (settingFields[1] != "null") {
                    switch (settingFields[0]) {
                        case "Name":
                            shellSettings.Name = settingFields[1];
                            break;
                        case "DataProvider":
                            shellSettings.DataProvider = settingFields[1];
                            break;
                        case "State":
                            shellSettings.State = new TenantState(settingFields[1]);
                            break;
                        case "DataConnectionString":
                            shellSettings.DataConnectionString = settingFields[1];
                            break;
                        case "DataPrefix":
                            shellSettings.DataTablePrefix = settingFields[1];
                            break;
                        case "RequestUrlHost":
                            shellSettings.RequestUrlHost = settingFields[1];
                            break;
                        case "RequestUrlPrefix":
                            shellSettings.RequestUrlPrefix = settingFields[1];
                            break;
                        case "EncryptionAlgorithm":
                            shellSettings.EncryptionAlgorithm = settingFields[1];
                            break;
                        case "EncryptionKey":
                            shellSettings.EncryptionKey = settingFields[1];
                            break;
                        case "EncryptionIV":
                            shellSettings.EncryptionIV = settingFields[1];
                            break;
                    }
                }
            }
            return shellSettings;
        }

        static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            return string.Format("Name: {0}\r\nDataProvider: {1}\r\nDataConnectionString: {2}\r\nDataPrefix: {3}\r\nRequestUrlHost: {4}\r\nRequestUrlPrefix: {5}\r\nState: {6}\r\nEncryptionAlgorithm: {7}\r\nEncryptionKey: {8}\r\nEncryptionIV: {9}\r\n",
                                 settings.Name,
                                 settings.DataProvider,
                                 settings.DataConnectionString ?? "null",
                                 settings.DataTablePrefix ?? "null",
                                 settings.RequestUrlHost ?? "null",
                                 settings.RequestUrlPrefix ?? "null",
                                 settings.State != null ? settings.State.ToString() : String.Empty,
                                 settings.EncryptionAlgorithm ?? "null",
                                 settings.EncryptionKey ?? "null",
                                 settings.EncryptionIV ?? "null"
                );
        }
    }
}
