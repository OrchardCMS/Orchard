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
            _appDataFolder.CreateFile(filePath, ShellSettingsSerializer.ComposeSettings(settings));
            _events.Saved(settings);
        }

        IEnumerable<ShellSettings> LoadSettings() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => string.Equals(Path.GetFileName(path), "Settings.txt", StringComparison.OrdinalIgnoreCase));

            foreach (var filePath in filePaths) {
                yield return ShellSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            }
        }
    }
}
