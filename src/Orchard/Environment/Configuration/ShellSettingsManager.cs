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

        Localizer T { get; set; }
        
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
                    }
                }
            }
            return shellSettings;
        }

        static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            return string.Format("Name: {0}\r\nDataProvider: {1}\r\nDataConnectionString: {2}\r\nDataPrefix: {3}\r\nRequestUrlHost: {4}\r\nRequestUrlPrefix: {5}\r\nState: {6}\r\n",
                     settings.Name,
                     settings.DataProvider,
                     settings.DataConnectionString ?? "null",
                     settings.DataTablePrefix ?? "null",
                     settings.RequestUrlHost ?? "null",
                     settings.RequestUrlPrefix ?? "null",
                     settings.State != null ? settings.State.ToString() : String.Empty);
        }
    }
}
