using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Orchard.FileSystems.VirtualPath {
    public class DefaultVirtualPathProvider : IVirtualPathProvider {
        public virtual string GetDirectoryName(string virtualPath) {
            return Path.GetDirectoryName(virtualPath).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual IEnumerable<string> ListFiles(string path) {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Files
                .OfType<VirtualFile>()
                .Select(f => ToAppRelative(f.VirtualPath));
        }

        public virtual IEnumerable<string> ListDirectories(string path) {
            return HostingEnvironment
                .VirtualPathProvider
                .GetDirectory(path)
                .Directories
                .OfType<VirtualDirectory>()
                .Select(d => ToAppRelative(d.VirtualPath));
        }

        public virtual string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public virtual string ToAppRelative(string virtualPath) {
            return VirtualPathUtility.ToAppRelative(virtualPath);
        }

        public virtual Stream OpenFile(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.GetFile(virtualPath).Open();
        }

        public virtual StreamWriter CreateText(string virtualPath) {
            return File.CreateText(MapPath(virtualPath));
        }

        public virtual Stream CreateFile(string virtualPath) {
            return File.Create(MapPath(virtualPath));
        }

        public virtual DateTime GetFileLastWriteTimeUtc(string virtualPath) {
            return File.GetLastWriteTimeUtc(MapPath(virtualPath));
        }

        public virtual string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public virtual bool FileExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.FileExists(virtualPath);
        }

        public virtual bool TryFileExists(string virtualPath) {
            try {
                // Check if the path falls outside the root directory of the app
                string directoryName = Path.GetDirectoryName(virtualPath);

                int level = 0;
                int stringLength = directoryName.Count();

                for(int i = 0 ; i < stringLength ; i++) {
                    if (directoryName[i] == '\\') {
                        if (i < (stringLength - 2) && directoryName[i + 1] == '.' && directoryName[i + 2] == '.') {
                            level--;
                            i += 2;
                        } else level++;
                    }

                    if (level < 0) {
                        return false;
                    }
                }

                return FileExists(virtualPath);
            }
            catch {
                return false;
            }
        }

        public virtual bool DirectoryExists(string virtualPath) {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath);
        }

        public virtual void CreateDirectory(string virtualPath) {
            Directory.CreateDirectory(MapPath(virtualPath));
        }
    }
}