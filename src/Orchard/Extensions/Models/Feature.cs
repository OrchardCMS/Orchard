using System;
using System.Collections.Generic;

namespace Orchard.Extensions.Models {
    public class Feature {
        public string ExtensionName { get; set; }
        public string Name { get; set; }
        public IEnumerable<Type> Types { get; set; }
        public FeatureDescriptor FeatureDescriptor { get; set; }
        public Extension Extension { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}
