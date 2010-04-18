using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Topology.Models {
    public class ShellTopologyDescriptor {
        public ShellTopologyDescriptor() {
            EnabledFeatures = Enumerable.Empty<TopologyFeature>();
            Parameters = Enumerable.Empty<TopologyParameter>();
        }

        public int SerialNumber { get; set; }
        public IEnumerable<TopologyFeature> EnabledFeatures { get; set; }
        public IEnumerable<TopologyParameter> Parameters { get; set; }
    }

    public class TopologyFeature {
        public string Name { get; set; }
    }

    public class TopologyParameter {
        public string Component { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
