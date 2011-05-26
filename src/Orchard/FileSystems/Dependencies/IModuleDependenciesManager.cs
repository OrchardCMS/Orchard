using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public interface IModuleDependenciesManager : IVolatileProvider {
        void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors);
        IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor descriptor);
    }
}