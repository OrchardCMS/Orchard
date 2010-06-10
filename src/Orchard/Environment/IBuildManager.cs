using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Compilation;

namespace Orchard.Environment {
    public interface IBuildManager : IDependency {
        IEnumerable<Assembly> GetReferencedAssemblies();
        Assembly GetCompiledAssembly(string virtualPath);
    }

    public class DefaultBuildManager : IBuildManager {
        public IEnumerable<Assembly> GetReferencedAssemblies() {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>();
        }

        public Assembly GetCompiledAssembly(string virtualPath) {
            return BuildManager.GetCompiledAssembly(virtualPath);
        }
    }
}