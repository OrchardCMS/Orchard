using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Orchard.Extensions.Helpers;
using Orchard.Extensions.Loaders;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Utility;
using Yaml.Grammar;
using System.Web;

namespace Orchard.Extensions {
    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private IEnumerable<ExtensionEntry> _activeExtensions;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ExtensionManager(
            IEnumerable<IExtensionFolders> folders,
            IEnumerable<IExtensionLoader> loaders) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }


        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            var availableExtensions = new List<ExtensionDescriptor>();
            foreach (var folder in _folders) {
                foreach (var name in folder.ListNames()) {
                    availableExtensions.Add(GetDescriptorForExtension(name, folder));
                }
            }
            return availableExtensions;
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
                HomePage = GetValue(fields, "homepage"),
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
                }
                featureDescriptors.Add(featureDescriptor);
         
            }
            return featureDescriptors;
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

        public IEnumerable<ExtensionEntry> ActiveExtensions() {
            if (_activeExtensions == null) {
                _activeExtensions = BuildActiveExtensions().ToList();
            }
            return _activeExtensions;
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
            //TODO: this component needs access to some "current settings" to know which are active
            foreach (var descriptor in AvailableExtensions()) {
                // Extensions that are Themes don't have buildable components.
                if (String.Equals(descriptor.ExtensionType, "Module", StringComparison.OrdinalIgnoreCase)) {
                    yield return BuildEntry(descriptor);
                }
            }
        }

        private ExtensionEntry BuildEntry(ExtensionDescriptor descriptor) {
            foreach (var loader in _loaders) {
                var entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }
            return null;
        }

    }

}
