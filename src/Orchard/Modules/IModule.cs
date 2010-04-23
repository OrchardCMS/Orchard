using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules {
    public interface IModule {
        string ModuleName { get; set; }
        string DisplayName { get; set; }
        string Description { get; set; }
        string Version { get; set; }
        string Author { get; set; }
        string HomePage { get; set; }
        string Tags { get; set; }
        IEnumerable<FeatureDescriptor> Features { get; set; }
    }
}