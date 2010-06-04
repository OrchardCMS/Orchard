using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Compilation;

namespace Orchard.Environment.Extensions.Loaders {
    public interface IBuildManager {
        IEnumerable<string> GetReferencedAssemblies();
    }

    public class AspNetBuildManager : IBuildManager {
        public IEnumerable<string> GetReferencedAssemblies() {
            return BuildManager.GetReferencedAssemblies().Cast<string>();
        }
    }
}