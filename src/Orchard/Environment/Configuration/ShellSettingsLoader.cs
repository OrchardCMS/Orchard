using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Yaml.Grammar;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsLoader {
        IEnumerable<IShellSettings> LoadSettings();
    }

    public class ShellSettingsLoader : IShellSettingsLoader {

        IEnumerable<IShellSettings> IShellSettingsLoader.LoadSettings() {
            return LoadSettings().ToArray();
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
    }
}
