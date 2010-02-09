using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Orchard.Localization;
using Yaml.Grammar;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsLoader {
        IEnumerable<IShellSettings> LoadSettings();
        void SaveSettings(IShellSettings settings);
    }

    public class ShellSettingsLoader : IShellSettingsLoader {
        Localizer T { get; set; }

        public ShellSettingsLoader() {
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

            var sitesPath = HostingEnvironment.MapPath("~/App_Data/Sites");

            if (string.IsNullOrEmpty(sitesPath))
                throw new ArgumentException(T("Can't determine the path on the server to save settings. Looking for something like \"~/App_Data/Sites\".").ToString());
            
            if (!Directory.Exists(sitesPath))
                Directory.CreateDirectory(sitesPath);

            var filePath = Path.Combine(sitesPath, string.Format("{0}.txt", settings.Name));
            File.WriteAllText(filePath, ComposeSettings(settings));
        }

        static IEnumerable<IShellSettings> LoadSettings() {
            foreach (var yamlDocument in LoadFiles()) {
                yield return ParseSettings(yamlDocument);
            }
        }

        static IEnumerable<YamlDocument> LoadFiles() {
            var sitesPath = HostingEnvironment.MapPath("~/App_Data/Sites");
            if (sitesPath != null && Directory.Exists(sitesPath)) {
                foreach (var settingsFilePath in Directory.GetFiles(sitesPath, "*.txt")) {
                    var yamlStream = YamlParser.Load(settingsFilePath);
                    yield return yamlStream.Documents.Single();
                }
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
                                         DataFolder = GetValue(fields, "DataFolder"),
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
            settingsBuilder.AppendLine(string.Format("DataFolder: {0}", shellSettings.DataFolder));
            settingsBuilder.AppendLine(string.Format("DataConnectionString: {0}", shellSettings.DataConnectionString));

            return settingsBuilder.ToString();
        }
    }
}
