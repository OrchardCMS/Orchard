using System;
using System.Collections.Generic;
using System.Reflection;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
    }

    public class ProbingAssembly {
        public string Path { get; set; }
        public Func<DateTime> LastWriteTimeUtc { get; set; }
        public Func<Assembly> Assembly { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        DependencyDescriptor GetDescriptor(string moduleName);
        IEnumerable<DependencyDescriptor> LoadDescriptors();
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);

        ProbingAssembly GetProbingAssembly(string moduleName);
        string GetProbingAssemblyPhysicalFileName(string moduleName);
    }
}
