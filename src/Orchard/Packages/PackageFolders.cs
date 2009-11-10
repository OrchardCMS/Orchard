using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Yaml.Grammar;

namespace Orchard.Packages {
    public interface IPackageFolders : IDependency {
        IEnumerable<string> ListNames();
        YamlDocument ParseManifest(string name);
    }

    public class PackageFolders : IPackageFolders {
        private readonly IEnumerable<string> _physicalPaths;

        public PackageFolders(IEnumerable<string> physicalPaths) {
            _physicalPaths = physicalPaths;
        }

        public IEnumerable<string> ListNames() {
            foreach (var path in _physicalPaths) {
                foreach (var directoryName in Directory.GetDirectories(path)) {
                    if (File.Exists(Path.Combine(directoryName, "Package.txt")))
                        yield return Path.GetFileName(directoryName);
                }
            }
        }

        public YamlDocument ParseManifest(string name) {
            foreach(var path in _physicalPaths) {
                var packageDirectoryPath = Path.Combine(path, name);
                var packageManifestPath = Path.Combine(packageDirectoryPath, "Package.txt");
                if (!File.Exists(packageManifestPath)) {
                    continue;
                }
                var yamlStream = YamlParser.Load(packageManifestPath);
                return yamlStream.Documents.Single();
            }
            return null;
        }
    }
}
