using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    public interface ICompositionStrategy {
        ShellTopology Compose(ShellTopologyDescriptor descriptor);
    }
}
