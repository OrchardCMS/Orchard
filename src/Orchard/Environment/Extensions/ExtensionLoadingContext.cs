using System.Collections.Generic;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions {
    public class ExtensionLoadingContext {
        public ExtensionLoadingContext() {
            FilesToDelete = new HashSet<string>();
            FilesToCopy = new Dictionary<string, string>();
            FilesToRename = new Dictionary<string, string>();
        }

        public IDependenciesFolder DependenciesFolder { get; set; }
        public HashSet<string> FilesToDelete { get; set; }
        public Dictionary<string, string> FilesToCopy { get; set; }
        public Dictionary<string, string> FilesToRename { get; set; }
        public bool RestartAppDomain { get; set; }
        public bool ResetSiteCompilation { get; set; }
    }
}