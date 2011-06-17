using Orchard.Caching;

namespace Orchard.AspNet.Abstractions {
    /// <summary>
    /// Enable monitoring changes over virtual path
    /// </summary>
    public interface IVirtualPathMonitor : ISingletonDependency {
        IVolatileToken WhenPathChanges(string virtualPath);
    }
}