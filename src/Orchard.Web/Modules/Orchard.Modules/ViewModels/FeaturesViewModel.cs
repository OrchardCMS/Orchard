using System.Collections.Generic;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel {
        public IEnumerable<IModuleFeature> Features { get; set; }
        public IEnumerable<string> FeaturesThatNeedUpdate { get; set; }
    }
}