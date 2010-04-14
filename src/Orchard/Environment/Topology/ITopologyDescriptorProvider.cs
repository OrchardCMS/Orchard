using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    /// <summary>
    /// Service resolved out of the shell container. Primarily used by host.
    /// </summary>
    public interface ITopologyDescriptorProvider {
        /// <summary>
        /// Uses shell-specific database or other resources to return 
        /// the current "correct" configuration. The host will use this information
        /// to reinitialize the shell.
        /// </summary>
        ShellTopologyDescriptor GetTopologyDescriptor();
    }
}
