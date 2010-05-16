using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.Environment.FileSystems {
    public interface IVirtualPathProvider : IVolatileProvider {
        IEnumerable<string> GetSubfolderPaths(string virtualPath);
        string ReadAllText(string virtualPath);

        IVolatileToken WhenPathChanges(string path);
    }
}
