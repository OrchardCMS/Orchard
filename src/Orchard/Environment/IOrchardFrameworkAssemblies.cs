using System.Collections.Generic;
using System.Reflection;

namespace Orchard.Environment {
    public interface IOrchardFrameworkAssemblies : IDependency {
        IEnumerable<AssemblyName> GetFrameworkAssemblies();
    }

    public class DefaultOrchardFrameworkAssemblies : IOrchardFrameworkAssemblies {
        public IEnumerable<AssemblyName> GetFrameworkAssemblies() {
            return typeof (IDependency).Assembly.GetReferencedAssemblies();
        }
    }
}
