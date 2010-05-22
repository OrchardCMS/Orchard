using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.WebSite {
    public interface IWebSiteFolder : IVolatileProvider {
        IEnumerable<string> ListDirectories(string path);
        string ReadFile(string path);

        IVolatileToken WhenPathChanges(string path);
    }
}