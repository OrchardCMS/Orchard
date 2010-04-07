using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orchard.Localization;
using Yaml.Grammar;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsLoader {
        IEnumerable<IShellSettings> LoadSettings();
        void SaveSettings(IShellSettings settings);
    }

    public class ShellSettingsLoader : IShellSettingsLoader {
        private readonly IAppDataFolder _appDataFolder;
        Localizer T { get; set; }

        public ShellSettingsLoader(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
            T = NullLocalizer.Instance;
        }

        IEnumerable<IShellSettings> IShellSettingsLoader.LoadSettings() {
            return LoadSettings().ToArray();
        }

        public void SaveSettings(IShellSettings settings) {
            if (settings == null)
                throw new ArgumentException(T("There are no settings to save.").ToString());
            if (string.IsNullOrEmpty(settings.Name))
                throw new ArgumentException(T("Settings \"Name\" is not set.").ToString());

            var settingsFile = Path.Combine(Path.Combine("Sites", settings.Name), "Settings.txt");
            _appDataFolder.CreateFile(settingsFile, ComposeSettings(settings));
        }

        IEnumerable<IShellSettings> LoadSettings() {
            foreach (var yamlDocument in LoadFiles()) {
                yield return ParseSettings(yamlDocument);
            }
        }

        IEnumerable<YamlDocument> LoadFiles() {
            var sitePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => string.Equals(Path.GetFileName(path), "Settings.txt", StringComparison.OrdinalIgnoreCase));

            foreach (var sitePath in sitePaths) {
                var yamlStream = YamlParser.Load(_appDataFolder.MapPath(sitePath));
                yield return yamlStream.Documents.Single();
            }
        }

        static IShellSettings ParseSettings(YamlDocument document) {
            var mapping = (Mapping)document.Root;
            var fields = mapping.Entities
                .Where(x => x.Key is Scalar)
                .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);

            return new ShellSettings {
                Name = GetValue(fields, "Name"),
                DataProvider = GetValue(fields, "DataProvider"),
                DataConnectionString = GetValue(fields, "DataConnectionString")
            };
        }

        static string GetValue(
            IDictionary<string, DataItem> fields,
            string key) {

            DataItem value;
            return fields.TryGetValue(key, out value) ? value.ToString() : null;
        }

        static string ComposeSettings(IShellSettings shellSettings) {
            if (shellSettings == null)
                return "";

            var settingsBuilder = new StringBuilder();

            settingsBuilder.AppendLine(string.Format("Name: {0}", shellSettings.Name));
            settingsBuilder.AppendLine(string.Format("DataProvider: {0}", shellSettings.DataProvider));

            if (!string.IsNullOrEmpty(shellSettings.DataConnectionString))
                settingsBuilder.AppendLine(string.Format("DataConnectionString: {0}", shellSettings.DataConnectionString));

            return settingsBuilder.ToString();
        }
    }
}
