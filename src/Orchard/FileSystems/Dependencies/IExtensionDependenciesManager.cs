using System;
using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public class ActivatedExtensionDescriptor {
        public string ExtensionId { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
        public string FileHash { get; set; }
    }

    public interface IExtensionDependenciesManager : IVolatileProvider {
        void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors, Func<string, string> fileHashProvider);

        IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor descriptor);
        ActivatedExtensionDescriptor GetDescriptor(string extensionId);
    }
}