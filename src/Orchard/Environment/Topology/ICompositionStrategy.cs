using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    /// <summary>
    /// Service at the host level to transform the cachable topology into the loadable topology.
    /// </summary>
    public interface ICompositionStrategy {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// topology model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        ShellTopology Compose(ShellTopologyDescriptor descriptor);
    }
}
