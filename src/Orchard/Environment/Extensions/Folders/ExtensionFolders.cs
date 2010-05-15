using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Environment.Extensions.Helpers;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.FileSystems;
using Yaml.Grammar;

namespace Orchard.Environment.Extensions.Folders {
    public class ParseResult {
        public string Location { get; set; }
        public string Name { get; set; }
        public YamlDocument YamlDocument { get; set; }
    }

    public class ExtensionFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly string _manifestName;
        private readonly string _extensionType;
        private readonly bool _manifestIsOptional;
        private readonly ICacheManager _cacheManager;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public ExtensionFolders(
            IEnumerable<string> paths,
            string manifestName,
            bool manifestIsOptional,
            ICacheManager cacheManager,
            IVirtualPathProvider virtualPathProvider) {
            _paths = paths;
            _manifestName = manifestName;
            _extensionType = manifestName == "Theme.txt" ? "Theme" : "Module";
            _manifestIsOptional = manifestIsOptional;
            _cacheManager = cacheManager;
            _virtualPathProvider = virtualPathProvider;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            var virtualPaths = _cacheManager.Get("", ctx => {
                var x = 5;
                return new[] { "hello" };
            });


            var list = new List<ExtensionDescriptor>();
            foreach (var virtualPath in virtualPaths) {
                list.Add(GetExtensionDescriptor(virtualPath));
            }
            return list;
        }

        ExtensionDescriptor GetExtensionDescriptor(string virtualPath) {
            return _cacheManager.Get(virtualPath, context => {

                context.IsInvalid(_virtualPathProvider.WhenPathChanges(virtualPath));

                var text = _virtualPathProvider.ReadAllText(virtualPath);

                var parseResult = ParseManifest(text);
                return GetDescriptorForExtension(parseResult.Name, parseResult);
            });
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


        private ExtensionDescriptor GetDescriptorForExtension(string name, ParseResult parseResult) {
            var mapping = (Mapping)parseResult.YamlDocument.Root;
            var fields = mapping.Entities
                .Where(x => x.Key is Scalar)
                .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);

            var extensionDescriptor = new ExtensionDescriptor {
                Location = parseResult.Location,
                Name = name,
                ExtensionType = _extensionType,
                DisplayName = GetValue(fields, "name") ?? name,
                Description = GetValue(fields, "description"),
                Version = GetValue(fields, "version"),
                OrchardVersion = GetValue(fields, "orchardversion"),
                Author = GetValue(fields, "author"),
                WebSite = GetValue(fields, "website"),
                Tags = GetValue(fields, "tags"),
                AntiForgery = GetValue(fields, "antiforgery"),
            };
            extensionDescriptor.Features = GetFeaturesForExtension(GetMapping(fields, "features"), extensionDescriptor);
            return extensionDescriptor;
        }

        private static IEnumerable<FeatureDescriptor> GetFeaturesForExtension(Mapping features, ExtensionDescriptor extensionDescriptor) {
            List<FeatureDescriptor> featureDescriptors = new List<FeatureDescriptor>();
            if (features != null) {
                foreach (var entity in features.Entities) {
                    FeatureDescriptor featureDescriptor = new FeatureDescriptor {
                        Extension = extensionDescriptor,
                        Name = entity.Key.ToString(),
                    };
                    Mapping featureMapping = (Mapping)entity.Value;
                    foreach (var featureEntity in featureMapping.Entities) {
                        if (String.Equals(featureEntity.Key.ToString(), "description", StringComparison.OrdinalIgnoreCase)) {
                            featureDescriptor.Description = featureEntity.Value.ToString();
                        }
                        else if (String.Equals(featureEntity.Key.ToString(), "category", StringComparison.OrdinalIgnoreCase)) {
                            featureDescriptor.Category = featureEntity.Value.ToString();
                        }
                        else if (String.Equals(featureEntity.Key.ToString(), "dependencies", StringComparison.OrdinalIgnoreCase)) {
                            featureDescriptor.Dependencies = ParseFeatureDependenciesEntry(featureEntity.Value.ToString());
                        }
                    }
                    featureDescriptors.Add(featureDescriptor);
                }
            }
            if (!featureDescriptors.Any(fd => fd.Name == extensionDescriptor.Name)) {
                featureDescriptors.Add(new FeatureDescriptor {
                    Name = extensionDescriptor.Name,
                    Dependencies = new string[0],
                    Extension = extensionDescriptor,
                });
            }
            return featureDescriptors;
        }

        private static string[] ParseFeatureDependenciesEntry(string dependenciesEntry) {
            List<string> dependencies = new List<string>();
            foreach (var s in dependenciesEntry.Split(',')) {
                dependencies.Add(s.Trim());
            }
            return dependencies.ToArray();
        }

        private static Mapping GetMapping(
            IDictionary<string, DataItem> fields,
            string key) {

            DataItem value;
            return fields.TryGetValue(key, out value) ? (Mapping)value : null;
        }

        private static string GetValue(
            IDictionary<string, DataItem> fields,
            string key) {

            DataItem value;
            return fields.TryGetValue(key, out value) ? value.ToString() : null;
        }
    }
}