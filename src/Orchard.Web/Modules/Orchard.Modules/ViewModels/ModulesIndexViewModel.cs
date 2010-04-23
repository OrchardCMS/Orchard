using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Modules.ViewModels {
    public class ModulesIndexViewModel : BaseViewModel {
        public IEnumerable<IModule> Modules { get; set; }
    }
}