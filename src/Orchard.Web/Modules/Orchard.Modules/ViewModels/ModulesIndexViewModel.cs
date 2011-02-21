using System.Collections.Generic;
using Orchard.Modules.Models;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel {
        public bool InstallModules { get; set; }
        public IEnumerable<ModuleEntry> Modules { get; set; }
    }
}