using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Yaml.Grammar;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsLoader {
        IEnumerable<IShellSettings> LoadSettings();
        bool SaveSettings(IShellSettings settings);
    }

    public class ShellSettingsLoader : IShellSettingsLoader {

        IEnumerable<IShellSettings> IShellSettingsLoader.LoadSettings() {
            return LoadSettings().ToArray();
        }

        public bool SaveSettings(IShellSettings settings) {
            if (settings != null && !string.IsNullOrEmpty(settings.Name)) {
                var sitesPath = HostingEnvironment.MapPath("~/App_Data/Sites");
                if (sitesPath != null) {
                    if (!Directory.Exists(sitesPath))
                        Directory.CreateDirectory(sitesPath);

                    var filePath = Path.Combine(sitesPath, string.Format("{0}.txt", settings.Name));
                    File.WriteAllText(filePath, ComposeSettings(settings));
                    return true;
                }
            }

            return false;
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

            return new ShellSettings { Name = GetValue(fields, "Name") };
        }

        static string GetValue(
            IDictionary<string, DataItem> fields,
            string key) {

            DataItem value;
            return fields.TryGetValue(key, out value) ? value.ToString() : null;
        }

        static string ComposeSettings(IShellSettings shellSettings) {
            return shellSettings == null ? "" : string.Format("Name: {0}", shellSettings.Name);
        }
    }
}
