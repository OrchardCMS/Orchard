using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public DependencyDescriptor() {
            References = Enumerable.Empty<DependencyReferenceDescriptor>();
        }
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
        public IEnumerable<DependencyReferenceDescriptor> References { get; set; }
    }

    public class DependencyReferenceDescriptor {
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        DependencyDescriptor GetDescriptor(string moduleName);
        IEnumerable<DependencyDescriptor> LoadDescriptors();
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);
    }
}
