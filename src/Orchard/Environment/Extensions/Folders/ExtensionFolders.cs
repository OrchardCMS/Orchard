using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;
using Orchard.Localization;
using Orchard.Logging;
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
        private readonly IWebSiteFolder _webSiteFolder;

        protected ExtensionFolders(
            IEnumerable<string> paths,
            string manifestName,
            bool manifestIsOptional,
            ICacheManager cacheManager,
            IWebSiteFolder webSiteFolder) {
            _paths = paths;
            _manifestName = manifestName;
            _extensionType = manifestName == "Theme.txt" ? "Theme" : "Module";
            _manifestIsOptional = manifestIsOptional;
            _cacheManager = cacheManager;
            _webSiteFolder = webSiteFolder;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }
        ILogger Logger { get; set; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            var list = new List<ExtensionDescriptor>();
            foreach (var locationPath in _paths) {
                var path = locationPath;
                var subList = _cacheManager.Get(locationPath, ctx => {
                    ctx.Monitor(_webSiteFolder.WhenPathChanges(ctx.Key));
                    var subfolderPaths = _webSiteFolder.ListDirectories(ctx.Key);
                    var localList = new List<ExtensionDescriptor>();
                    foreach ( var subfolderPath in subfolderPaths ) {
                        var extensionName = Path.GetFileName(subfolderPath.TrimEnd('/', '\\'));
                        var manifestPath = Path.Combine(subfolderPath, _manifestName);
                        ctx.Monitor(_webSiteFolder.WhenPathChanges(manifestPath));
                        try {
                            var descriptor = GetExtensionDescriptor(path, extensionName, manifestPath);
                            if ( descriptor != null )
                                localList.Add(descriptor);
                        }
                        catch ( Exception ex ) {
                            // Ignore invalid module manifests
                            Logger.Error(ex, "A module could not be loaded. It was ignored.");
                        }
                    }
                    return localList;                                              
                });
                list.AddRange(subList);
            }

            return list;
        }

        private ExtensionDescriptor GetExtensionDescriptor(string locationPath, string extensionName, string manifestPath) {
            return _cacheManager.Get(manifestPath, context => {

                context.Monitor(_webSiteFolder.WhenPathChanges(manifestPath));

                var manifestText = _webSiteFolder.ReadFile(manifestPath);
                if (manifestText == null) {
                    if (_manifestIsOptional) {
                        manifestText = string.Format("Name: {0}", extensionName);
                    }
                    else {
                        return null;
                    }
                }

                return GetDescriptorForExtension(locationPath, extensionName, ParseManifest(manifestText));
            });
        }

        private ExtensionDescriptor GetDescriptorForExtension(string locationPath, string extensionName, ParseResult parseResult) {
            return GetDescriptorForExtension(locationPath, extensionName, _extensionType, parseResult);
        }

        public static ParseResult ParseManifest(string manifestText) {
            bool success;
            var yamlStream = new YamlParser().ParseYamlStream(new TextInput(manifestText), out success);
            if (yamlStream == null || !success) {
                return null;
            }
            return new ParseResult {
                Name = manifestText,
                YamlDocument = yamlStream.Documents.Single()
            };
        }

        public static ExtensionDescriptor GetDescriptorForExtension(string locationPath, string extensionName, string extensionType, ParseResult parseResult) {
            var mapping = (Mapping)parseResult.YamlDocument.Root;
            var fields = mapping.Entities
                .Where(x => x.Key is Scalar)
                .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);

            var extensionDescriptor = new ExtensionDescriptor {
                Location = locationPath,
                Name = extensionName,
                ExtensionType = extensionType,
                DisplayName = GetValue(fields, "Name") ?? extensionName,
                Description = GetValue(fields, "Description"),
                Version = GetValue(fields, "Version"),
                OrchardVersion = GetValue(fields, "OrchardVersion"),
                Author = GetValue(fields, "Author"),
                WebSite = GetValue(fields, "Website"),
                Tags = GetValue(fields, "Tags"),
                AntiForgery = GetValue(fields, "AntiForgery"),
                Zones = GetValue(fields, "Zones"),
            };

            extensionDescriptor.Features = GetFeaturesForExtension(GetMapping(fields, "Features"), extensionDescriptor);

            return extensionDescriptor;
        }

        private static IEnumerable<FeatureDescriptor> GetFeaturesForExtension(Mapping features, ExtensionDescriptor extensionDescriptor) {
            var featureDescriptors = new List<FeatureDescriptor>();
            if (features != null) {
                foreach (var entity in features.Entities) {
                    var featureDescriptor = new FeatureDescriptor {
                        Extension = extensionDescriptor,
                        Name = entity.Key.ToString(),
                    };
                    var featureMapping = (Mapping)entity.Value;
                    foreach (var featureEntity in featureMapping.Entities) {
                        if (String.Equals(featureEntity.Key.ToString(), "Description", StringComparison.OrdinalIgnoreCase)) {
                            featureDescriptor.Description = featureEntity.Value.ToString();
                        }
                        else if (String.Equals(featureEntity.Key.ToString(), "Category", StringComparison.OrdinalIgnoreCase)) {
                            featureDescriptor.Category = featureEntity.Value.ToString();
                        }
                        else if (String.Equals(featureEntity.Key.ToString(), "Dependencies", StringComparison.OrdinalIgnoreCase)) {
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
                    Extension = extensionDescriptor
                });
            }
            return featureDescriptors;
        }

        private static string[] ParseFeatureDependenciesEntry(string dependenciesEntry) {
            var dependencies = new List<string>();
            foreach (var s in dependenciesEntry.Split(',')) {
                dependencies.Add(s.Trim());
            }
            return dependencies.ToArray();
        }

        private static Mapping GetMapping(IDictionary<string, DataItem> fields, string key) {
            DataItem value;
            return fields.TryGetValue(key, out value) ? (Mapping)value : null;
        }

        private static string GetValue(IDictionary<string, DataItem> fields, string key) {
            DataItem value;
            return fields.TryGetValue(key, out value) ? value.ToString() : null;
        }
    }
}