using System.Collections.Generic;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel {
        public IEnumerable<IModule> Modules { get; set; }
    }
}