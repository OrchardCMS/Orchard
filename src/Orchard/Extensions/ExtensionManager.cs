using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Extensions.Loaders;
using Yaml.Grammar;

namespace Orchard.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<ExtensionEntry> ActiveExtensions();
    }

    public class ExtensionManager : IExtensionManager {
        private readonly IEnumerable<IExtensionFolders> _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private IEnumerable<ExtensionEntry> _activeExtensions;

        public ExtensionManager(IEnumerable<IExtensionFolders> folders, IEnumerable<IExtensionLoader> loaders) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
        }


        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            List<ExtensionDescriptor> availableExtensions = new List<ExtensionDescriptor>();
            foreach (var folder in _folders) {
                foreach (var name in folder.ListNames()) {
                    availableExtensions.Add(GetDescriptorForExtension(name, folder));
                }
            }
            return availableExtensions;
        }

        private static ExtensionDescriptor GetDescriptorForExtension(string name, IExtensionFolders folder) {
            string extensionType = folder is ThemeFolders ? "Theme" : "Package";
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
                Author = GetValue(fields, "author"),
                HomePage = GetValue(fields, "homepage")
            };
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

        private IEnumerable<ExtensionEntry> BuildActiveExtensions() {
            //TODO: this component needs access to some "current settings" to know which are active
            foreach (var descriptor in AvailableExtensions()) {
                // Extensions that are Themes don't have buildable components.
                if (String.Equals(descriptor.ExtensionType, "Package", StringComparison.OrdinalIgnoreCase)) {
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
