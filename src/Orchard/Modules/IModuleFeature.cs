using System;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules {
    [Obsolete]
    public interface IModuleFeature {
        [Obsolete]
        FeatureDescriptor Descriptor { get; set; }
        [Obsolete]
        bool IsEnabled { get; set; }
    }
}