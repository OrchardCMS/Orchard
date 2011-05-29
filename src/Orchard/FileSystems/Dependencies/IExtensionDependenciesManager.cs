using System;
using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public interface IExtensionDependenciesManager : IVolatileProvider {
        void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors, Func<string, string> fileHashProvider);
        IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor descriptor);
    }
}