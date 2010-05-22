using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel : BaseViewModel {
        public IEnumerable<IModuleFeature> Features { get; set; }
    }
}