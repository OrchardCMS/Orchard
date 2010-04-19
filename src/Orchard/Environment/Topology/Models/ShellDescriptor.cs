using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Topology.Models {
    public class ShellDescriptor {
        public ShellDescriptor() {
            EnabledFeatures = Enumerable.Empty<ShellFeature>();
            Parameters = Enumerable.Empty<ShellParameter>();
        }

        public int SerialNumber { get; set; }
        public IEnumerable<ShellFeature> EnabledFeatures { get; set; }
        public IEnumerable<ShellParameter> Parameters { get; set; }
    }

    public class ShellFeature {
        public string Name { get; set; }
    }

    public class ShellParameter {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
