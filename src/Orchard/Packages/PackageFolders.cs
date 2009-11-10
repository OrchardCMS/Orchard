using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Yaml.Grammar;

namespace Orchard.Packages {
    public interface IPackageFolders {
        IEnumerable<string> ListNames();
        YamlDocument ParseManifest(string name);
    }

    public class PackageFolders : IPackageFolders {
        private readonly IEnumerable<string> _paths;

        public PackageFolders(IEnumerable<string> paths) {
            _paths = paths;
        }

        private IEnumerable<string> GetPhysicalPaths() {
            foreach(var path in _paths) {
                if (path.StartsWith("~") && HostingEnvironment.IsHosted) {
                    yield return HostingEnvironment.MapPath(path);
                }
                else {
                    yield return path;
                }
            }
        }


        public IEnumerable<string> ListNames() {
            foreach (var path in GetPhysicalPaths()) {
                foreach (var directoryName in Directory.GetDirectories(path)) {
                    if (File.Exists(Path.Combine(directoryName, "Package.txt")))
                        yield return Path.GetFileName(directoryName);
                }
            }
        }

        public YamlDocument ParseManifest(string name) {
            foreach (var path in GetPhysicalPaths()) {
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
