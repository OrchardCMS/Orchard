using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Orchard.Packages.Loaders;
using Yaml.Grammar;

namespace Orchard.Packages {
    public interface IPackageManager {
        IEnumerable<PackageDescriptor> AvailablePackages();
        IEnumerable<PackageEntry> ActivePackages();
    }

    public class PackageManager : IPackageManager {
        private readonly IPackageFolders _folders;
        private readonly IEnumerable<IPackageLoader> _loaders;
        private IEnumerable<PackageEntry> _activePackages;

        public PackageManager(IPackageFolders folders, IEnumerable<IPackageLoader> loaders) {
            _folders = folders;
            _loaders = loaders.OrderBy(x => x.Order);
        }


        public IEnumerable<PackageDescriptor> AvailablePackages() {
            var names = _folders.ListNames();
            foreach (var name in names) {
                var parseResult = _folders.ParseManifest(name);
                var mapping = (Mapping)parseResult.YamlDocument.Root;
                var fields = mapping.Entities
                    .Where(x => x.Key is Scalar)
                    .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);


                yield return new PackageDescriptor {
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

        public IEnumerable<PackageEntry> ActivePackages() {
            if (_activePackages == null) {
                _activePackages = BuildActivePackages().ToList();
            }
            return _activePackages;
        }

        private IEnumerable<PackageEntry> BuildActivePackages() {
            foreach (var descriptor in AvailablePackages()) {
                //TODO: this component needs access to some "current settings" to know which are active
                yield return BuildEntry(descriptor);
            }
        }

        private PackageEntry BuildEntry(PackageDescriptor descriptor) {
            foreach (var loader in _loaders) {
                var entry = loader.Load(descriptor);
                if (entry != null)
                    return entry;
            }
            return null;
        }

    }

}
