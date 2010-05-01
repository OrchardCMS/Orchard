using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules {
    public interface IModuleFeature {
        FeatureDescriptor Descriptor { get; set; }
        bool IsEnabled { get; set; }
    }
}