using System.Reflection;
using Orchard.Caching;
using Orchard.Environment;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public string ModuleName { get; set; }
        public bool IsFromBuildProvider { get; set; }
        public string VirtualPath { get; set; }
        public string FileName { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        void StoreReferencedAssembly(string moduleName);
        void StorePrecompiledAssembly(string moduleName, string virtualPath);
        void StoreBuildProviderAssembly(string moduleName, string virtualPath, Assembly assembly);
        DependencyDescriptor GetDescriptor(string moduleName);
        Assembly LoadAssembly(string assemblyName);
    }
}
