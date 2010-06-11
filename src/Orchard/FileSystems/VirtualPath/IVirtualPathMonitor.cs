using System;
using Orchard.Caching;

namespace Orchard.FileSystems.VirtualPath {
    /// <summary>
    /// Enable monitoring changes over virtual path
    /// </summary>
    public interface IVirtualPathMonitor : IVolatileProvider {
        IVolatileToken WhenPathChanges(string virtualPath);
        // Temporary until we have a generic mechanism for components
        // to synchronize their dependencies through a Context.Monitor()
        // interface
        void WhenPathChanges(string virtualPath, Action action);
    }
}