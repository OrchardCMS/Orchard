using System;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Models {
    public class Feature {
        public FeatureDescriptor FeatureDescriptor { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}