using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Orchard.FileSystems.VirtualPath {
    public class DefaultVirtualPathProvider : IVirtualPathProvider {
        public string GetDirectoryName(string virtualPath) {
            return Path.GetDirectoryName(virtualPath).Replace(Path.DirectorySeparatorChar, '/');
        }

        public IEnumerable<string> ListFiles(string path) {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Files
                .OfType<VirtualFile>()
                .Select(f => ToAppRelative(f.VirtualPath));
        }

        public IEnumerable<string> ListDirectories(string path) {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Directories
                .OfType<VirtualDirectory>()
                .Select(d => ToAppRelative(d.VirtualPath));
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public string ToAppRelative(string virtualPath) {
            return VirtualPathUtility.ToAppRelative(virtualPath);
        }

        public Stream OpenFile(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
        }

        public StreamWriter CreateText(string virtualPath) {
            return File.CreateText(MapPath(virtualPath));
        }

        public Stream CreateFile(string virtualPath) {
            return File.Create(MapPath(virtualPath));
        }

        public DateTime GetFileLastWriteTimeUtc(string virtualPath) {
            return File.GetLastWriteTimeUtc(MapPath(virtualPath));
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public bool FileExists(string virtualPath) {
            try {
                return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
            } catch {
                // Medium Trust or invalid mappings that fall outside the app folder
                return false;
            }
        }

        public bool DirectoryExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath);
        }

        public void CreateDirectory(string virtualPath) {
            Directory.CreateDirectory(MapPath(virtualPath));
        }
    }
}