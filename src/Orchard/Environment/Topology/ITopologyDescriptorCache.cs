using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    /// <summary>
    /// Single service instance registered at the host level. Provides storage
    /// and recall of topology descriptor information. Default implementation uses
    /// app_data, but configured replacements could use other per-host writable location.
    /// </summary>
    public interface ITopologyDescriptorCache {
        /// <summary>
        /// Recreate the named configuration information. Used at startup. 
        /// Returns null on cache-miss.
        /// </summary>
        ShellTopologyDescriptor Fetch(string name);

        /// <summary>
        /// Commit named configuration to reasonable persistent storage.
        /// This storage is scoped to the current-server and current-webapp.
        /// Loss of storage is expected.
        /// </summary>
        void Store(string name, ShellTopologyDescriptor descriptor);
    }
}
