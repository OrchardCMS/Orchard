using System.IO;
using Orchard.Caching;

namespace Orchard.FileSystems.VirtualPath {
    public interface IVirtualPathProvider : IVolatileProvider {
        string Combine(params string[] paths);
        string MapPath(string virtualPath);

        bool FileExists(string virtualPath);
        Stream OpenFile(string virtualPath);
        StreamWriter CreateText(string virtualPath);

        bool DirectoryExists(string virtualPath);
        void CreateDirectory(string virtualPath);
        string GetDirectoryName(string virtualPath);
    }
}