using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.FileSystems.AppData;
using Orchard.Localization;

namespace Orchard.Environment.Configuration {
    public class ShellSettingsManager : IShellSettingsManager {
        private const string _settingsFileName = "Settings.txt";
        private readonly IAppDataFolder _appDataFolder;
        private readonly IShellSettingsManagerEventHandler _events;

        public Localizer T {
            get;
            set;
        }

        public ShellSettingsManager(
            IAppDataFolder appDataFolder,
            IShellSettingsManagerEventHandler events) {
            _appDataFolder = appDataFolder;
            _events = events;

            T = NullLocalizer.Instance;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var settings = LoadSettingsInternal().ToArray();
            PlatformShellSettings.ApplyTo(settings); // Apply platform configuration overrides.
            return settings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentNullException("settings");
            if (String.IsNullOrEmpty(settings.Name))
                throw new ArgumentException("The Name property of the supplied ShellSettings object is null or empty. The settings cannot be saved.", "settings");

            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), _settingsFileName);
            _appDataFolder.CreateFile(filePath, ShellSettingsSerializer.ComposeSettings(settings));
            _events.Saved(settings);
        }

        private IEnumerable<ShellSettings> LoadSettingsInternal() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => String.Equals(Path.GetFileName(path), _settingsFileName, StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in filePaths) {
                yield return ShellSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            }
        }
    }
}
