using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.FileSystems.AppData;
using Orchard.Localization;

namespace Orchard.Environment.Configuration {
    public class ShellSettingsManager : IShellSettingsManager {
        private readonly IAppDataFolder _appDataFolder;
        private readonly IShellSettingsManagerEventHandler _events;
        public const char Separator = ':';
        public const string EmptyValue = "null";

        public Localizer T { get; set; }
        
        public ShellSettingsManager(
            IAppDataFolder appDataFolder, 
            IShellSettingsManagerEventHandler events) {
            _appDataFolder = appDataFolder;
            _events = events;

            T = NullLocalizer.Instance;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            return LoadSettings().ToArray();
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentException(T("There are no settings to save.").ToString());
            if (string.IsNullOrEmpty(settings.Name))
                throw new ArgumentException(T("Settings \"Name\" is not set.").ToString());

            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), "Settings.txt");
            _appDataFolder.CreateFile(filePath, ComposeSettings(settings));
            _events.Saved(settings);
        }

        IEnumerable<ShellSettings> LoadSettings() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => string.Equals(Path.GetFileName(path), "Settings.txt", StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in filePaths) {
                yield return ParseSettings(_appDataFolder.ReadFile(filePath));
            }
        }

        static ShellSettings ParseSettings(string text) {
            var shellSettings = new ShellSettings();
            if (String.IsNullOrEmpty(text))
                return shellSettings;

            var settings = new StringReader(text);
            string setting;
            while ((setting = settings.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(setting)) continue; ;
                var separatorIndex = setting.IndexOf(Separator);
                if (separatorIndex == -1)
                {
                    continue;
                }
                string key = setting.Substring(0, separatorIndex).Trim();
                string value = setting.Substring(separatorIndex + 1).Trim();

                if (value != EmptyValue)
                {
                    switch (key)
                    {
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
