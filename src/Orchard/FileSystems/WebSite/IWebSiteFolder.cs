using System;
using System.Collections.Generic;
using Orchard.Caching;

namespace Orchard.FileSystems.WebSite {
    /// <summary>
    /// Abstraction over the virtual files/directories of a web site.
    /// </summary>
    public interface IWebSiteFolder : IVolatileProvider {
        IEnumerable<string> ListDirectories(string virtualPath);
        string ReadFile(string virtualPath);

        IVolatileToken WhenPathChanges(string virtualPath);
        void WhenPathChanges(string virtualPath, Action action);
    }
}