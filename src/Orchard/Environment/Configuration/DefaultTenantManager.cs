using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Yaml.Serialization;
using Orchard.Localization;

namespace Orchard.Environment.Configuration {
    public class DefaultTenantManager : ITenantManager {
        private readonly IAppDataFolder _appDataFolder;
        Localizer T { get; set; }

        public DefaultTenantManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
        }

        IEnumerable<ShellSettings> ITenantManager.LoadSettings() {
            return LoadSettings().ToArray();
        }

        void ITenantManager.SaveSettings(ShellSettings settings) {
            if (settings == null)
                throw new ArgumentException(T("There are no settings to save.").ToString());
            if (string.IsNullOrEmpty(settings.Name))
                throw new ArgumentException(T("Settings \"Name\" is not set.").ToString());

            var filePath = Path.Combine(Path.Combine("Sites", settings.Name), "Settings.txt");
            _appDataFolder.CreateFile(filePath, ComposeSettings(settings));
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

        class Content {
            public string Name { get; set; }
            public string DataProvider { get; set; }
            public string DataConnectionString { get; set; }
            public string DataPrefix { get; set; }
        }

        static ShellSettings ParseSettings(string text) {
            var ser = new YamlSerializer();
            var content = ser.Deserialize(text, typeof(Content)).Cast<Content>().Single();
            return new ShellSettings {
                Name = content.Name,
                DataProvider = content.DataProvider,
                DataConnectionString = content.DataConnectionString,
                DataPrefix = content.DataPrefix,
            };
        }

        static string ComposeSettings(ShellSettings settings) {
            if (settings == null)
                return "";

            var ser = new YamlSerializer();
            return ser.Serialize(new Content {
                Name = settings.Name,
                DataProvider = settings.DataProvider,
                DataConnectionString = settings.DataConnectionString,
                DataPrefix = settings.DataPrefix,
            });
        }
    }
}
