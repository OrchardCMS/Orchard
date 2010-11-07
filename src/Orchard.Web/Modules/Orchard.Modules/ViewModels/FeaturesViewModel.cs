using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel {
        public IEnumerable<ModuleFeature> Features { get; set; }
    }
}

