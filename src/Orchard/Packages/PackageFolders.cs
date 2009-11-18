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
        ParseResult ParseManifest(string name);
    }

    public class ParseResult {
        public string Location { get; set; }
        public string Name { get; set; }
        public YamlDocument YamlDocument { get; set; }
    }

    public class PackageFolders : IPackageFolders {
        private readonly IEnumerable<string> _paths;

        public PackageFolders(IEnumerable<string> paths) {
            _paths = paths;
        }

        static string GetPhysicalPath(string path) {
            if (path.StartsWith("~") && HostingEnvironment.IsHosted) {
                return HostingEnvironment.MapPath(path);
            }

            return path;

        }



        public IEnumerable<string> ListNames() {
            foreach (var path in _paths) {
                foreach (var directoryName in Directory.GetDirectories(GetPhysicalPath(path))) {
                    if (File.Exists(Path.Combine(directoryName, "Package.txt")))
                        yield return Path.GetFileName(directoryName);
                }
            }
        }

        public ParseResult ParseManifest(string name) {
            foreach (var path in _paths) {
                var packageDirectoryPath = Path.Combine(GetPhysicalPath(path), name);
                var packageManifestPath = Path.Combine(packageDirectoryPath, "Package.txt");
                if (!File.Exists(packageManifestPath)) {
                    continue;
                }
                var yamlStream = YamlParser.Load(packageManifestPath);
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
