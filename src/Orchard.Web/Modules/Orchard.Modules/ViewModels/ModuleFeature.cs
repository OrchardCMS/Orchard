using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.ViewModels {
    public class ModuleFeature {
        public FeatureDescriptor Descriptor  { get; set; }
        public bool IsEnabled { get; set; }
        public bool NeedsUpdate { get; set; }
    }
}