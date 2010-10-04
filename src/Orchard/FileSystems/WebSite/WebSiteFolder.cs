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
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IVirtualPathMonitor _virtualPathMonitor;

        public WebSiteFolder(IVirtualPathMonitor virtualPathMonitor, IVirtualPathProvider virtualPathProvider) {
            _virtualPathMonitor = virtualPathMonitor;
            _virtualPathProvider = virtualPathProvider;
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

        private IEnumerable<string> ListFiles(IEnumerable<string> directories) {
            return from dir in directories
                   from file in ListFiles(dir, true)
                   select file;
        }

        public IEnumerable<string> ListFiles(string virtualPath, bool recursive) {
            if (!recursive) {
                return from VirtualFile file in HostingEnvironment.VirtualPathProvider.GetDirectory(virtualPath).Files
                       select file.VirtualPath;
            }
            return (from VirtualFile file in HostingEnvironment.VirtualPathProvider.GetDirectory(virtualPath).Files
                    select file.VirtualPath).Concat(ListFiles(ListDirectories(virtualPath)));
        }
        
        public bool FileExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        public string ReadFile(string virtualPath) {
            return ReadFile(virtualPath, false);
        }

        public string ReadFile(string virtualPath, bool actualContent) {
            if (!HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
                return null;

            if (actualContent) {
                var physicalPath = _virtualPathProvider.MapPath(virtualPath);
                using (var stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read)) {
                    using (var reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
            else {
                using (var stream = VirtualPathProvider.OpenFile(Normalize(virtualPath))) {
                    using (var reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public void CopyFileTo(string virtualPath, Stream destination) {
            CopyFileTo(virtualPath, destination, false/*actualContent*/);
        }

        public void CopyFileTo(string virtualPath, Stream destination, bool actualContent) {
            if (actualContent) {
                // This is an unfortunate side-effect of the dynamic compilation work.
                // Orchard has a custom virtual path provider which adds "<@Assembly xxx@>"
                // directives to WebForm view files. There are cases when this side effect
                // is not expected by the consumer of the WebSiteFolder API.
                // The workaround here is to go directly to the file system.
                var physicalPath = _virtualPathProvider.MapPath(virtualPath);
                using (var stream = File.Open(physicalPath, FileMode.Open, FileAccess.Read)) {
                    stream.CopyTo(destination);
                }
            }
            else {
                using (var stream = VirtualPathProvider.OpenFile(Normalize(virtualPath))) {
                    stream.CopyTo(destination);
                }
            }
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            return _virtualPathMonitor.WhenPathChanges(virtualPath);
        }

        static string Normalize(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).VirtualPath;
        }
    }
}