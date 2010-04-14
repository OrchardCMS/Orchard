using System.Collections.Generic;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    public class DefaultTopologyDescriptorCache : ITopologyDescriptorCache {
        readonly IDictionary<string, ShellTopologyDescriptor> _cache= new Dictionary<string, ShellTopologyDescriptor>();

        public ShellTopologyDescriptor Fetch(string name) {
            ShellTopologyDescriptor value;
            return _cache.TryGetValue(name, out value) ? value : null;
        }

        public void Store(string name, ShellTopologyDescriptor descriptor) {
            _cache[name] = descriptor;
        }
    }
}
