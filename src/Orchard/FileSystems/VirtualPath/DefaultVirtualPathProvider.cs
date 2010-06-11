using System.IO;
using System.Web.Hosting;

namespace Orchard.FileSystems.VirtualPath {
    public class DefaultVirtualPathProvider : IVirtualPathProvider {
        public string GetDirectoryName(string virtualPath) {
            return Path.GetDirectoryName(virtualPath).Replace(Path.DirectorySeparatorChar, '/');
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
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