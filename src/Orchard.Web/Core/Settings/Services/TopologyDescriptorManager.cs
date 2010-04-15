using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;

namespace Orchard.Core.Settings.Services {
    public class TopologyDescriptorManager : ITopologyDescriptorManager {
        public ShellTopologyDescriptor GetTopologyDescriptor() {
            throw new NotImplementedException();
        }

        public void UpdateTopologyDescriptor(int priorSerialNumber, IEnumerable<TopologyFeature> enabledFeatures, IEnumerable<TopologyParameter> parameters) {
            throw new NotImplementedException();
        }
    }
}
