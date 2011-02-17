using System.Collections.Generic;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel {
        public bool InstallModules { get; set; }
        public IEnumerable<Module> Modules { get; set; }
    }
}