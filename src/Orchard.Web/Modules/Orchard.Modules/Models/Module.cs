using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Modules.Models {
    public class Module : IModule {
        public string ModuleName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string HomePage { get; set; }
        public string Tags { get; set; }
        public IEnumerable<FeatureDescriptor> Features { get; set; }
    }
}