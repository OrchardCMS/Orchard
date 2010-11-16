using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NuGet;

namespace Orchard.Packaging.Services {
    public class FileBasedProjectSystem : IProjectSystem {
        private const string NetFrameworkIdentifier = ".NETFramework";
        private const string BinDir = "bin";
        private string _root;

        public FileBasedProjectSystem(string root) {
            if (String.IsNullOrEmpty(root)) {
                throw new ArgumentException("root");
            }
            _root = root;
        }

        public string Root {
            get {
                return _root;
            }
        }

        public virtual string ProjectName {
            get {
                return Root;
            }
        }
        
        private FrameworkName _targetFramework;
        private ILogger _logger;

        public ILogger Logger {
            get {
                return _logger ?? NullLogger.Instance;
            }
            set {
                _logger = value;
            }
        }

        public virtual FrameworkName TargetFramework {
            get {
                if (_targetFramework == null) {
                    _targetFramework = new FrameworkName(NetFrameworkIdentifier, typeof(string).Assembly.GetNameSafe().Version);;
                }
                return _targetFramework;
            }
        }

        public string GetFullPath(string path) {
            return Path.Combine(Root, path);
        }

        protected virtual string GetReferencePath(string name) {
            return Path.Combine(BinDir, name);
        }

        public void AddFile(string path, Stream stream) {
            EnsureDirectory(Path.GetDirectoryName(path));

            using (Stream outputStream = File.Create(GetFullPath(path))) {
                stream.CopyTo(outputStream);
            }

        }

        public void DeleteFile(string path) {
            if (!FileExists(path)) {
                return;
            }

            try {
                path = GetFullPath(path);
                File.Delete(path);
            }
            catch (FileNotFoundException) {}
        }

        public void DeleteDirectory(string path) {
            DeleteDirectory(path, recursive: false);
        }

        public void DeleteDirectory(string path, bool recursive) {
            if (!DirectoryExists(path)) {
                return;
            }

            try {
                path = GetFullPath(path);
                Directory.Delete(path, recursive);
            }
            catch (DirectoryNotFoundException) {

            }
        }

        public void AddReference(string referencePath) {
            // Copy to bin by default
            string src = referencePath;
            string referenceName = Path.GetFileName(referencePath);
            string dest = GetFullPath(GetReferencePath(referenceName));

            // Ensure the destination path exists
            Directory.CreateDirectory(Path.GetDirectoryName(dest));

            // Copy the reference over
            File.Copy(src, dest, overwrite: true);
        }

        public void RemoveReference(string name) {
            DeleteFile(GetReferencePath(name));

            // Delete the bin directory if this was the last reference
            if (!GetFiles(BinDir).Any()) {
                DeleteDirectory(BinDir);
            }
        }

        public dynamic GetPropertyValue(string propertyName) {
            if(propertyName == null) {
                return null;
            }

            // Return empty string for the root namespace of this project.
            if (propertyName.Equals("RootNamespace", StringComparison.OrdinalIgnoreCase)) {
                return String.Empty;
            }

            return null;
        }

        public IEnumerable<string> GetFiles(string path) {
            return GetFiles(path, "*.*");
        }

        public IEnumerable<string> GetFiles(string path, string filter) {
            path = EnsureTrailingSlash(GetFullPath(path));
            try {
                if (!Directory.Exists(path)) {
                    return Enumerable.Empty<string>();
                }
                return Directory.EnumerateFiles(path, filter)
                                .Select(MakeRelativePath);
            }
            catch (UnauthorizedAccessException) {

            }
            catch (DirectoryNotFoundException) {

            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetDirectories(string path) {
            try {
                path = EnsureTrailingSlash(GetFullPath(path));
                if (!Directory.Exists(path)) {
                    return Enumerable.Empty<string>();
                }
                return Directory.EnumerateDirectories(path)
                                .Select(MakeRelativePath);
            }
            catch (UnauthorizedAccessException) {

            }
            catch (DirectoryNotFoundException) {
                
            }

            return Enumerable.Empty<string>();
        }

        public DateTimeOffset GetLastModified(string path) {
            if (DirectoryExists(path)) {
                return new DirectoryInfo(GetFullPath(path)).LastWriteTimeUtc;
            }
            return new FileInfo(GetFullPath(path)).LastWriteTimeUtc;
        }

        public bool FileExists(string path) {
            path = GetFullPath(path);
            return File.Exists(path);
        }

        public bool DirectoryExists(string path) {
            path = GetFullPath(path);
            return Directory.Exists(path);
        }

        public Stream OpenFile(string path) {
            path = GetFullPath(path);
            return File.OpenRead(path);
        }

        public bool ReferenceExists(string name) {
            string path = GetReferencePath(name);
            return FileExists(path);
        }

        public virtual bool IsSupportedFile(string path) {
            return true;
        }

        protected string MakeRelativePath(string fullPath) {
            return fullPath.Substring(Root.Length).TrimStart(Path.DirectorySeparatorChar);
        }

        private void EnsureDirectory(string path) {
            path = GetFullPath(path);
            Directory.CreateDirectory(path);
        }

        private static string EnsureTrailingSlash(string path) {
            if (!path.EndsWith("\\", StringComparison.Ordinal)) {
                path += "\\";
            }
            return path;
        }
    }
}
