using System.IO;
using System.Web.Hosting;
using Orchard.Caching;

namespace Orchard.Environment {
    public interface IVirtualPathProvider : IVolatileProvider {
        string GetDirectoryName(string virtualPath);
        string Combine(params string[] paths);
        Stream OpenFile(string virtualPath);
        StreamWriter CreateText(string virtualPath);
        string MapPath(string virtualPath);
        bool FileExists(string virtualPath);
        bool DirectoryExists(string virtualPath);
        void CreateDirectory(string virtualPath);
    }

    public class DefaultVirtualPathProvider : IVirtualPathProvider {
        public string GetDirectoryName(string virtualPath) {
            return Path.GetDirectoryName(virtualPath).Replace('\\', '/');
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace('\\', '/');
        }

        public Stream OpenFile(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
        }

        public StreamWriter CreateText(string virtualPath) {
            return File.CreateText(MapPath(virtualPath));
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public bool FileExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        public bool DirectoryExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath);
        }

        public void CreateDirectory(string virtualPath) {
            Directory.CreateDirectory(MapPath(virtualPath));
        }
    }
}