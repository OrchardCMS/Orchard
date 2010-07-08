using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.FileSystems.WebSite {
    public class WebSiteFolder : IWebSiteFolder {
        private readonly IVirtualPathMonitor _virtualPathMonitor;

        public WebSiteFolder(IVirtualPathMonitor virtualPathMonitor) {
            _virtualPathMonitor = virtualPathMonitor;
        }

        public IEnumerable<string> ListDirectories(string virtualPath) {
            if (!HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath))
                return Enumerable.Empty<string>();

            return HostingEnvironment.VirtualPathProvider
                .GetDirectory(virtualPath)
                .Directories.OfType<VirtualDirectory>()
                .Select(d => d.VirtualPath)
                .ToArray();
        }
        
        public bool FileExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        public string ReadFile(string virtualPath) {
            if (!HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
                return null;

            using (var stream = VirtualPathProvider.OpenFile(Normalize(virtualPath))) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public void CopyFileTo(string virtualPath, Stream destination) {
            using (var stream = VirtualPathProvider.OpenFile(Normalize(virtualPath))) {
                stream.CopyTo(destination);
            }
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            return _virtualPathMonitor.WhenPathChanges(virtualPath);
        }

        public void WhenPathChanges(string virtualPath, Action action) {
            _virtualPathMonitor.WhenPathChanges(virtualPath, action);
        }

        static string Normalize(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).VirtualPath;
        }
    }
}