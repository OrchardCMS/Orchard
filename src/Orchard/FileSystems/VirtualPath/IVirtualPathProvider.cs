using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Caching;

namespace Orchard.FileSystems.VirtualPath {
    public interface IVirtualPathProvider : ISingletonDependency {
        string Combine(params string[] paths);
        string ToAppRelative(string virtualPath);
        string ToAbsolute(string virtualPath);
        string MapPath(string virtualPath);

        bool IsAppRelative(string virtualPath);
        bool IsAbsolute(string virtualPath);


        bool FileExists(string virtualPath);
        bool TryFileExists(string virtualPath);
        Stream OpenFile(string virtualPath);
        StreamWriter CreateText(string virtualPath);
        Stream CreateFile(string virtualPath);
        DateTime GetFileLastWriteTimeUtc(string virtualPath);
        string GetFileHash(string virtualPath);
        string GetFileHash(string virtualPath, IEnumerable<string> dependencies);

        bool DirectoryExists(string virtualPath);
        void CreateDirectory(string virtualPath);
        string GetDirectoryName(string virtualPath);

        IEnumerable<string> ListFiles(string path);
        IEnumerable<string> ListDirectories(string path);
    }
}