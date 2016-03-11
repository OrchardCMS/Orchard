using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace Orchard.Environment.Configuration {

    public class ShellSettingsManager : Component, IShellSettingsManager {

        private const string SettingsFileName = "Settings.txt";
        private readonly IAppDataFolder _appDataFolder;
        private readonly IShellSettingsManagerEventHandler _events;

        public ShellSettingsManager(IAppDataFolder appDataFolder, IShellSettingsManagerEventHandler events) {
            _appDataFolder = appDataFolder;
            _events = events;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            Logger.Information("Reading ShellSettings...");
            var settingsList = LoadSettingsInternal().ToArray();

            var tenantNamesQuery =
                from settings in settingsList
                select settings.Name;
            Logger.Information("Returning {0} ShellSettings objects for tenants {1}.", tenantNamesQuery.Count(), String.Join(", ", tenantNamesQuery));
            
            return settingsList;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (String.IsNullOrEmpty(settings.Name))
                throw new ArgumentException("The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.", "settings");

            Logger.Information("Saving ShellSettings for tenant '{0}'...", settings.Name);
            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), SettingsFileName);
            _appDataFolder.CreateFile(filePath, ShellSettingsSerializer.ComposeSettings(settings));

            Logger.Information("ShellSettings saved successfully; flagging tenant '{0}' for restart.", settings.Name);
            _events.Saved(settings);
        }

        private IEnumerable<ShellSettings> LoadSettingsInternal() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => String.Equals(Path.GetFileName(path), SettingsFileName, StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in filePaths) {
                yield return ShellSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            }
        }
    }
}
