using System;
using System.IO;
using System.Web.Hosting;

namespace Orchard.Environment.Extensions.Loaders {
    public interface IVirtualPathProvider {
        string GetDirectoryName(string virtualPath);
        string Combine(params string[] paths);
        Stream OpenFile(string virtualPath);
        string MapPath(string virtualPath);
    }

    public class AspNetVirtualPathProvider : IVirtualPathProvider {
        public string GetDirectoryName(string virtualPath) {
            return Path.GetDirectoryName(virtualPath);
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace('\\', '/');
        }

        public Stream OpenFile(string virtualPath) {
            return VirtualPathProvider.OpenFile(virtualPath);
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}