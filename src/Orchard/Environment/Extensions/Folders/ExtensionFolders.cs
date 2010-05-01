using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions.Helpers;
using Yaml.Grammar;

namespace Orchard.Environment.Extensions.Folders {
    public class ExtensionFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly string _manifestName;
        private readonly bool _manifestIsOptional;

        public ExtensionFolders(IEnumerable<string> paths, string manifestName, bool manifestIsOptional) {
            _paths = paths;
            _manifestName = manifestName;
            _manifestIsOptional = manifestIsOptional;
        }

        public IEnumerable<string> ListNames() {
            foreach (var path in _paths) {
                if (!Directory.Exists(PathHelpers.GetPhysicalPath(path)))
                    continue;

                foreach (var directoryName in Directory.GetDirectories(PathHelpers.GetPhysicalPath(path))) {
                    if (_manifestIsOptional || File.Exists(Path.Combine(directoryName, _manifestName))) {
                        yield return Path.GetFileName(directoryName);
                    }
                }
            }
        }

        public ParseResult ParseManifest(string name) {
            foreach (var path in _paths) {
                if (!Directory.Exists(PathHelpers.GetPhysicalPath(path)))
                    continue;

                var extensionDirectoryPath = Path.Combine(PathHelpers.GetPhysicalPath(path), name);
                if (!Directory.Exists(PathHelpers.GetPhysicalPath(extensionDirectoryPath)))
                    continue;

                var extensionManifestPath = Path.Combine(extensionDirectoryPath, _manifestName);

                if (File.Exists(extensionManifestPath)) {
                    var yamlStream = YamlParser.Load(extensionManifestPath);
                    return new ParseResult {
                                               Location = path,
                                               Name = name,
                                               YamlDocument = yamlStream.Documents.Single()
                                           };
                }

                if (_manifestIsOptional) {
                    var yamlInput = new TextInput(string.Format("name: {0}", name));
                    var parser = new YamlParser();
                    bool success;
                    var yamlStream = parser.ParseYamlStream(yamlInput, out success);
                    return new ParseResult {
                                               Location = path,
                                               Name = name,
                                               YamlDocument = yamlStream.Documents.Single()
                                           };
                }
            }
            return null;
        }
    }
}