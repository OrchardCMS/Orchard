using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Modules.ViewModels {
    public class FeaturesViewModel : BaseViewModel {
        public IEnumerable<IModuleFeature> Features { get; set; }
        public FeaturesOptions Options { get; set; }
    }

    public class FeaturesOptions {
        public FeaturesBulkAction BulkAction { get; set; }
    }

    public enum FeaturesBulkAction {
        None,
        Enable,
        Disable
    }
}