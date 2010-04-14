using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Orchard.Specs.Util {
    public class PathUtil {
        private readonly string _path;

        public PathUtil(string path) {
            _path = path;
        }

        public static PathUtil GetTempFile() {
            return new PathUtil(Path.GetTempFileName());
        }

        public static PathUtil GetTempFolder() {
            var path = GetTempFile().DeleteFile().CreateDirectory();
            return path;
        }

        public static PathUtil BaseDirectory {
            get { return new PathUtil(AppDomain.CurrentDomain.BaseDirectory); }
        }

        public PathUtil FullPath {
            get { return new PathUtil(Path.GetFullPath(_path)); }
        }

        public PathUtil CreateDirectory() {
            Directory.CreateDirectory(_path);
            return this;
        }

        public PathUtil DeleteFile() {
            File.Delete(_path);
            return this;
        }

        public PathUtil Combine(PathUtil path) {
            return Combine(path.ToString());
        }
        public PathUtil Combine(string path) {
            return new PathUtil(Path.Combine(_path, path));
        }

        public IEnumerable<PathUtil> GetFiles(string pattern) {
            return GetFiles(pattern, SearchOption.TopDirectoryOnly);
        }

        public IEnumerable<PathUtil> GetFiles(SearchOption searchOptions) {
            return GetFiles("*", searchOptions);
        }

        public IEnumerable<PathUtil> GetFiles(string pattern, SearchOption searchOptions) {
            return Directory.GetFiles(_path, pattern, searchOptions).Select(sz => new PathUtil(sz));
        }

        public void CopyAll(string pattern, PathUtil target) {
            CopyAll(pattern, SearchOption.TopDirectoryOnly, target);
        }

        public void CopyAll(SearchOption searchOptions, PathUtil target) {
            CopyAll("*", searchOptions, target);
        }

        public void CopyAll(string pattern, SearchOption searchOptions, PathUtil target) {
            foreach (var file in GetFiles(pattern, searchOptions)) {
                var targetFile = target.Combine(file.GetRelativePath(this));
                targetFile.Parent.CreateDirectory();
                file.Copy(targetFile);
            }
        }

        public override int GetHashCode() {
            return (_path ?? string.Empty).GetHashCode();
        }
        public override bool Equals(object obj) {
            if (obj == null || obj.GetType() != GetType())
                return false;
            return ((PathUtil)obj)._path == _path;
        }
        public override string ToString() {
            return _path;
        }

        public PathUtil GetRelativePath(PathUtil basePath) {
            if (this.Equals(basePath))
                return new PathUtil(".");

            if (Parent.Equals(basePath))
                return FileName;

            return Parent.GetRelativePath(basePath).Combine(FileName);
        }


        public PathUtil Copy(PathUtil target) {
            File.Copy(_path, target._path, true);
            return target;
        }

        public PathUtil Parent {
            get { return new PathUtil(Path.GetDirectoryName(_path)); }
        }

        public PathUtil FileName {
            get { return new PathUtil(Path.GetFileName(_path)); }
        }

        public bool DirectoryExists {
            get { return Directory.Exists(_path); }
        }
    }
}
