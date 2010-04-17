using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Orchard.Environment;
using Orchard.Extensions.Helpers;
using Orchard.Extensions.Loaders;
using Orchard.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Yaml.Grammar;
using System.Web;

namespace Orchard.Extensions {
    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private IEnumerable<ExtensionEntry> _activeExtensions;
        //private readonly IRepository<ExtensionRecord> _extensionRepository;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionManager(
            IEnumerable<IExtensionFolders> folders,
            IEnumerable<IExtensionLoader> loaders
            //IRepository<ExtensionRecord> extensionRepository
            ) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
            //_extensionRepository = extensionRepository;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        // This method does not load extension types, simply parses extension manifests from 
        // the filesystem. 
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            var availableExtensions = new List<ExtensionDescriptor>();
            foreach (var folder in _folders) {
                foreach (var name in folder.ListNames()) {
                    availableExtensions.Add(GetDescriptorForExtension(name, folder));
                }
            }
            return availableExtensions;
        }

        public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> features) {
            throw new NotImplementedException();
        }

        // This method loads types from extensions into the ExtensionEntry array.
        public IEnumerable<ExtensionEntry> ActiveExtensions() {
            if (_activeExtensions == null) {
                _activeExtensions = BuildActiveExtensions().ToList();
            }
            return _activeExtensions;
        }
      

        private static ExtensionDescriptor GetDescriptorForExtension(string name, IExtensionFolders folder) {
            string extensionType = folder is ThemeFolders ? "Theme" : "Module";
            var parseResult = folder.ParseManifest(name);
            var mapping = (Mapping)parseResult.YamlDocument.Root;
            var fields = mapping.Entities
                .Where(x => x.Key is Scalar)
                .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);

            return new ExtensionDescriptor {
                Location = parseResult.Location,
                Name = name,
                ExtensionType = extensionType,
                DisplayName = GetValue(fields, "name"),
                Description = GetValue(fields, "description"),
                Version = GetValue(fields, "version"),
                OrchardVersion = GetValue(fields, "orchardversion"),
                Author = GetValue(fields, "author"),
                WebSite = GetValue(fields, "website"),
                Tags = GetValue(fields, "tags"),
                AntiForgery = GetValue(fields, "antiforgery"),
                Features = GetFeaturesForExtension(GetMapping(fields, "features"), name),
            };
        }

        private static IEnumerable<FeatureDescriptor> GetFeaturesForExtension(Mapping features, string name) {
            List<FeatureDescriptor> featureDescriptors = new List<FeatureDescriptor>();
            if (features == null) return featureDescriptors;
            foreach (var entity in features.Entities) {
                FeatureDescriptor featureDescriptor = new FeatureDescriptor {
                    ExtensionName = name,
                    Name = entity.Key.ToString(),
                };
                Mapping featureMapping = (Mapping) entity.Value;
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
            return featureDescriptors;
        }

        public ShellTopology_Obsolete GetExtensionsTopology() {
            var types = ActiveExtensions().SelectMany(x => x.ExportedTypes);
            types = types.Concat(typeof(IOrchardHost).Assembly.GetExportedTypes());
            return new ShellTopology_Obsolete { Types = types.Where(t => t.IsClass && !t.IsAbstract) };
        }

        public IEnumerable<Type> LoadFeature(string featureName) {
            string extensionName = GetExtensionForFeature(featureName);
            if (extensionName == null) throw new ArgumentException(T("Feature ") + featureName + T(" was not found in any of the installed extensions"));
            var extension = ActiveExtensions().Where(x => x.Descriptor.Name == extensionName).FirstOrDefault();
            if (extension == null) throw new InvalidOperationException(T("Extension ") + extensionName + T(" is not active"));

            var extensionTypes = extension.ExportedTypes.Where(t => t.IsClass && !t.IsAbstract);
            var featureTypes = new List<Type>();

            foreach (var type in extensionTypes) {
                string sourceFeature = GetSourceFeatureNameForType(type, extensionName);
                if (String.Equals(sourceFeature, featureName, StringComparison.OrdinalIgnoreCase)) {
                    featureTypes.Add(type);
                }
            }

            return featureTypes;
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionName) {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetCustomAttributes(typeof(OrchardFeatureAttribute), false)) {
                return featureAttribute.FeatureName;
            }
            return extensionName;
        }

        private string GetExtensionForFeature(string featureName) {
            foreach (var extensionDescriptor in AvailableExtensions()) {
                if (String.Equals(extensionDescriptor.Name, featureName, StringComparison.OrdinalIgnoreCase)) {
                    return extensionDescriptor.Name;
                }
                foreach (var feature in extensionDescriptor.Features) {
                    if (String.Equals(feature.Name, featureName, StringComparison.OrdinalIgnoreCase)) {
                        return extensionDescriptor.Name;
                    }
                }
            }
            return null;
        }

        public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
            if (String.IsNullOrEmpty(extensionType)) {
                throw new ArgumentException(T("extensionType was null or empty").ToString());
            }
            string targetFolder;
            if (String.Equals(extensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Themes");
            }
            else if (String.Equals(extensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Modules");
            }
            else {
                throw new ArgumentException(T("extensionType was not recognized").ToString());
            }
            int postedFileLength = extensionBundle.ContentLength;
            Stream postedFileStream = extensionBundle.InputStream;
            byte[] postedFileData = new byte[postedFileLength];
            postedFileStream.Read(postedFileData, 0, postedFileLength);

            using (var memoryStream = new MemoryStream(postedFileData)) {
                var fileInflater = new ZipInputStream(memoryStream);
                ZipEntry entry;
                while ((entry = fileInflater.GetNextEntry()) != null) {
                    string directoryName = Path.GetDirectoryName(entry.Name);
                    if (!Directory.Exists(Path.Combine(targetFolder, directoryName))) {
                        Directory.CreateDirectory(Path.Combine(targetFolder, directoryName));
                    }

                    if (!entry.IsDirectory && entry.Name.Length > 0) {
                        var len = Convert.ToInt32(entry.Size);
                        var extractedBytes = new byte[len];
                        fileInflater.Read(extractedBytes, 0, len);
                        File.WriteAllBytes(Path.Combine(targetFolder, entry.Name), extractedBytes);
                    }
                }
            }
        }

        public void UninstallExtension(string extensionType, string extensionName) {
            if (String.IsNullOrEmpty(extensionType)) {
                throw new ArgumentException(T("extensionType was null or empty").ToString());
            }
            string targetFolder;
            if (String.Equals(extensionType, "Theme", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Themes");
            }
            else if (String.Equals(extensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                targetFolder = PathHelpers.GetPhysicalPath("~/Modules");
            }
            else {
                throw new ArgumentException(T("extensionType was not recognized").ToString());
            }
            targetFolder = Path.Combine(targetFolder, extensionName);
            if (!Directory.Exists(targetFolder)) {
                throw new ArgumentException(T("extension was not found").ToString());
            }
            Directory.Delete(targetFolder, true);
        }

        private IEnumerable<ExtensionEntry> BuildActiveExtensions() {
            foreach (var descriptor in AvailableExtensions()) {
                //_extensionRepository.Create(new ExtensionRecord { Name = descriptor.Name });
                // Extensions that are Themes don't have buildable components.
                if (String.Equals(descriptor.ExtensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                    yield return BuildEntry(descriptor);
                }
            }
        }

        private bool IsExtensionEnabled(string name) {
            //ExtensionRecord extensionRecord = _extensionRepository.Get(x => x.Name == name);
            //if (extensionRecord.Enabled) return true;
            //return false;
            return true;
        }

        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor) {
            if (!IsExtensionEnabled(descriptor.Name)) return null; 
            foreach (var loader in _loaders) {
                var entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }
            return null;
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
