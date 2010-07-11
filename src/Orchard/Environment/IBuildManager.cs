using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Environment {
    public interface IBuildManager : IDependency {
        IEnumerable<Assembly> GetReferencedAssemblies();
        bool HasReferencedAssembly(string name);
        Assembly GetReferencedAssembly(string name);
        Assembly GetCompiledAssembly(string virtualPath);
    }

    public class DefaultBuildManager : IBuildManager {
        private readonly IVirtualPathProvider _virtualPathProvider;

        public DefaultBuildManager(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;
        }

        public IEnumerable<Assembly> GetReferencedAssemblies() {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>();
        }

        public bool HasReferencedAssembly(string name) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", name + ".dll");
            return _virtualPathProvider.FileExists(assemblyPath);
        }

        public Assembly GetReferencedAssembly(string name) {
            if (!HasReferencedAssembly(name))
                return null;

            return Assembly.Load(name);
        }


        public Assembly GetCompiledAssembly(string virtualPath) {
            return BuildManager.GetCompiledAssembly(virtualPath);
        }
    }

    public static class BuildManagerExtensions {
        public static IEnumerable<string> GetReferencedAssemblyNames(this IBuildManager buildManager) {
            return buildManager
                .GetReferencedAssemblies()
                .Select(a => ExtractAssemblyName(a.FullName));
        }

        public static string ExtractAssemblyName(string fullName) {
            int index = fullName.IndexOf(',');
            if (index < 0)
                return fullName;
            return fullName.Substring(0, index);
        }
    }
}