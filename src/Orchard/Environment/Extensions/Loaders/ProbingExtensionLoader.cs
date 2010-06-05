using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension using the "Assembly.Load" method if the
    /// file can be found in the "App_Data/Dependencies" folder.
    /// </summary>
    public class ProbingExtensionLoader : IExtensionLoader {
        private readonly IDependenciesFolder _folder;

        public ProbingExtensionLoader(IDependenciesFolder folder) {
            _folder = folder;
        }

        public int Order { get { return 30; } }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var assembly = _folder.LoadAssembly(descriptor.Name);
            if (assembly == null)
                return null;

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}