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
        private readonly string _root;

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
            get { return _targetFramework ?? (_targetFramework = new FrameworkName(NetFrameworkIdentifier, typeof (string).Assembly.GetNameSafe().Version)); }
        }

        public string GetFullPath(string path) {
            return Path.Combine(Root, Uri.UnescapeDataString(path));
        }

        protected virtual string GetReferencePath(string name) {
            return Path.Combine(BinDir, name);
        }

        public void AddFile(string path, Stream stream) {
            string fullPath = GetFullPath(path);
            EnsureDirectory(Path.GetDirectoryName(fullPath));

            using (Stream outputStream = File.Create(fullPath)) {
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
            DeleteDirectory(path, false);
        }

        public void DeleteDirectory(string path, bool recursive) {
            if (!DirectoryExists(path)) {
                return;
            }

            try {
                path = GetFullPath(path);
                Directory.Delete(path, recursive);
            }
            catch (DirectoryNotFoundException) {}
        }

        public void AddReference(string referencePath, Stream stream) {
            // Not used by Orchard
            throw new NotSupportedException();
        }

        public void RemoveReference(string name) {
            // Not used by Orchard
            throw new NotSupportedException();
        }

        public dynamic GetPropertyValue(string propertyName) {
            if(propertyName == null) {
                return null;
            }

            // Return empty string for the root namespace of this project.
            return propertyName.Equals("RootNamespace", StringComparison.OrdinalIgnoreCase) ? String.Empty : null;
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
            catch (UnauthorizedAccessException) {}
            catch (DirectoryNotFoundException) {}

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
            catch (UnauthorizedAccessException) {}
            catch (DirectoryNotFoundException) {}

            return Enumerable.Empty<string>();
        }

        public DateTimeOffset GetLastModified(string path) {
            return DirectoryExists(path) ? new DirectoryInfo(GetFullPath(path)).LastWriteTimeUtc : new FileInfo(GetFullPath(path)).LastWriteTimeUtc;
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

        public DateTimeOffset GetCreated(string path) {
            if (this.DirectoryExists(path)) {
                return Directory.GetCreationTimeUtc(this.GetFullPath(path));
            }

            return File.GetCreationTimeUtc(this.GetFullPath(path));
        }
    }
}
