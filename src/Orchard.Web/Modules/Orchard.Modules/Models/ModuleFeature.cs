using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.Models {
    public class ModuleFeature : IModuleFeature {
        public FeatureDescriptor Descriptor  { get; set; }
        public bool IsEnabled { get; set; }
    }
}