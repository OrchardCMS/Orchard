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
        private readonly IExtensionFolders _folders;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private IEnumerable<ExtensionEntry> _activeExtensions;

        public ExtensionManager(IExtensionFolders folders, IEnumerable<IExtensionLoader> loaders) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
        }


        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            var names = _folders.ListNames();
            foreach (var name in names) {
                var parseResult = _folders.ParseManifest(name);
                var mapping = (Mapping)parseResult.YamlDocument.Root;
                var fields = mapping.Entities
                    .Where(x => x.Key is Scalar)
                    .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);


                yield return new ExtensionDescriptor {
                    Location = parseResult.Location,
                    Name = name,
                    DisplayName = GetValue(fields, "name"),
                    Description = GetValue(fields, "description"),
                    Version = GetValue(fields, "version")
                };
            }
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
            foreach (var descriptor in AvailableExtensions()) {
                //TODO: this component needs access to some "current settings" to know which are active
                yield return BuildEntry(descriptor);
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
