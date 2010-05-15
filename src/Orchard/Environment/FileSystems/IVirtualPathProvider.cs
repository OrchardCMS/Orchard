using Orchard.Caching.Providers;

namespace Orchard.Environment.FileSystems {
    public interface IVirtualPathProvider : IVolatileProvider {
        IVolatileSignal WhenPathChanges(string path);

        string ReadAllText(string virtualPath);
    }
}