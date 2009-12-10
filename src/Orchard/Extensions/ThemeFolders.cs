using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Extensions.Helpers;
using Yaml.Grammar;

namespace Orchard.Extensions {
    public class ThemeFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;

        public ThemeFolders(IEnumerable<string> paths) {
            _paths = paths;
        }

        public IEnumerable<string> ListNames() {
            foreach (var path in _paths) {
                foreach (var directoryName in Directory.GetDirectories(PathHelpers.GetPhysicalPath(path))) {
                    if (File.Exists(Path.Combine(directoryName, "Theme.txt")))
                        yield return Path.GetFileName(directoryName);
                }
            }
        }

        public ParseResult ParseManifest(string name) {
            foreach (var path in _paths) {
                var extensionDirectoryPath = Path.Combine(PathHelpers.GetPhysicalPath(path), name);
                var extensionManifestPath = Path.Combine(extensionDirectoryPath, "Theme.txt");
                if (!File.Exists(extensionManifestPath)) {
                    continue;
                }
                var yamlStream = YamlParser.Load(extensionManifestPath);
                return new ParseResult {
                    Location = path,
                    Name = name,
                    YamlDocument = yamlStream.Documents.Single()
                };
            }
            return null;
        }
    }
}
