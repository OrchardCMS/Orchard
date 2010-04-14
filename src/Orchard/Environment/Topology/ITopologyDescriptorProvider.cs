using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    public interface ITopologyDescriptorProvider {
        ShellTopologyDescriptor GetTopologyDescriptor();
    }
}
