using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Yaml.Grammar;

namespace Orchard.Packages {
    public interface IPackageManager {
        IEnumerable<PackageDescriptor> AvailablePackages();
        IEnumerable<PackageEntry> ActivePackages();
    }

    public class PackageManager : IPackageManager {
        private readonly IPackageFolders _folders;

        public PackageManager(IPackageFolders folders) {
            _folders = folders;
        }


        public IEnumerable<PackageDescriptor> AvailablePackages() {
            var names = _folders.ListNames();
            foreach (var name in names) {
                var document = _folders.ParseManifest(name);
                var mapping = (Mapping)document.Root;
                var fields = mapping.Entities
                    .Where(x => x.Key is Scalar)
                    .ToDictionary(x => ((Scalar)x.Key).Text, x => x.Value);


                yield return new PackageDescriptor {
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
            foreach (var descriptor in AvailablePackages()) {
                //TODO: this component needs access to some "current settings" to know which are active
                yield return BuildEntry(descriptor);
            }
        }

        private static PackageEntry BuildEntry(PackageDescriptor descriptor) {
            var entry = new PackageEntry {
                Descriptor = descriptor,
                Assembly = Assembly.Load(descriptor.Name)
            };
            return entry;
        }
    }

}
