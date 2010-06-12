using System.Reflection;
using Orchard.Caching;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public string ModuleName { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
        public string FileName { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        void Store(DependencyDescriptor descriptor);
        void StorePrecompiledAssembly(string moduleName, string virtualPath, string loaderName);
        void Remove(string moduleName, string loaderName);
        DependencyDescriptor GetDescriptor(string moduleName);
        bool HasPrecompiledAssembly(string moduleName);
        Assembly LoadAssembly(string moduleName);
    }
}
