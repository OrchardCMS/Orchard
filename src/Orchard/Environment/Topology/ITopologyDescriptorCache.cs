using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    public interface ITopologyDescriptorCache {
        ShellTopologyDescriptor Fetch(string name);
        void Store(string name, ShellTopologyDescriptor descriptor);
    }
}
