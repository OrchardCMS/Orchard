#if !AZURE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment.Configuration;
using Orchard.Localization;

namespace Orchard.FileSystems.Media {
    public class FileSystemStorageProvider : IStorageProvider {
        private readonly string _storagePath;
        private readonly string _publicPath;

        public FileSystemStorageProvider(ShellSettings settings) {
            var mediaPath = HostingEnvironment.IsHosted
                                ? HostingEnvironment.MapPath("~/Media/") ?? ""
                                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");
            
            _storagePath = Path.Combine(mediaPath, settings.Name);

            var appPath = "";
            if (HostingEnvironment.IsHosted) {
                appPath = HostingEnvironment.ApplicationVirtualPath;
            }
            if (!appPath.EndsWith("/"))
                appPath = appPath + '/';
            if (!appPath.StartsWith("/"))
                appPath = '/' + appPath;

            _publicPath = appPath + "Media/" + settings.Name + "/";

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        string Map(string path) {
            return string.IsNullOrEmpty(path) ? _storagePath : Path.Combine(_storagePath, path);
        }

        static string Fix(string path) {
            return string.IsNullOrEmpty(path)
                       ? ""
                       : Path.DirectorySeparatorChar != '/'
                             ? path.Replace('/', Path.DirectorySeparatorChar)
                             : path;
        }

        #region Implementation of IStorageProvider

        public string GetPublicUrl(string path) {

            return Map(_publicPath + path.Replace(Path.DirectorySeparatorChar, '/'));
        }

        public IStorageFile GetFile(string path) {
            if (!File.Exists(Map(path))) {
                throw new ArgumentException(T("File {0} does not exist", path).ToString());
            }
            return new FileSystemStorageFile(Fix(path), new FileInfo(Map(path)));
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            if (!Directory.Exists(Map(path))) {
                throw new ArgumentException(T("Directory {0} does not exist", path).ToString());
            }

            return new DirectoryInfo(Map(path))
                .GetFiles()
                .Where(fi => !IsHidden(fi))
                .Select<FileInfo, IStorageFile>(fi => new FileSystemStorageFile(Path.Combine(Fix(path), fi.Name), fi))
                .ToList();
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {
            if (!Directory.Exists(Map(path))) {
                try {
                    Directory.CreateDirectory(Map(path));
                }
                catch (Exception ex) {
                    throw new ArgumentException(T("The folder could not be created at path: {0}. {1}", path, ex).ToString());
                }
            }

            return new DirectoryInfo(Map(path))
                .GetDirectories()
                .Where(di => !IsHidden(di))
                .Select<DirectoryInfo, IStorageFolder>(di => new FileSystemStorageFolder(Path.Combine(Fix(path), di.Name), di))
                .ToList();
        }

        private static bool IsHidden(FileSystemInfo di) {
            return (di.Attributes & FileAttributes.Hidden) != 0;
        }

        public void CreateFolder(string path) {
            if (Directory.Exists(Map(path))) {
                throw new ArgumentException(T("Directory {0} already exists", path).ToString());
            }

            Directory.CreateDirectory(Map(path));
        }

        public void DeleteFolder(string path) {
            if (!Directory.Exists(Map(path))) {
                throw new ArgumentException(T("Directory {0} does not exist", path).ToString());
            }

            Directory.Delete(Map(path), true);
        }

        public void RenameFolder(string path, string newPath) {
            if (!Directory.Exists(Map(path))) {
                throw new ArgumentException(T("Directory {0} does not exist", path).ToString());
            }

            if (Directory.Exists(Map(newPath))) {
                throw new ArgumentException(T("Directory {0} already exists", newPath).ToString());
            }

            Directory.Move(Map(path), Map(newPath));
        }

        public IStorageFile CreateFile(string path) {
            if (File.Exists(Map(path))) {
                throw new ArgumentException(T("File {0} already exists", path).ToString());
            }

            var fileInfo = new FileInfo(Map(path));
            File.WriteAllBytes(Map(path), new byte[0]);

            return new FileSystemStorageFile(Fix(path), fileInfo);
        }

        public void DeleteFile(string path) {
            if (!File.Exists(Map(path))) {
                throw new ArgumentException(T("File {0} does not exist", path).ToString());
            }

            File.Delete(Map(path));
        }

        public void RenameFile(string path, string newPath) {
            if (!File.Exists(Map(path))) {
                throw new ArgumentException(T("File {0} does not exist", path).ToString());
            }

            if (File.Exists(Map(newPath))) {
                throw new ArgumentException(T("File {0} already exists", newPath).ToString());
            }

            File.Move(Map(path), Map(newPath));
        }

        public string Combine(string path1, string path2) {
            return Path.Combine(path1, path2);
        }

        #endregion

        private class FileSystemStorageFile : IStorageFile {
            private readonly string _path;
            private readonly FileInfo _fileInfo;

            public FileSystemStorageFile(string path, FileInfo fileInfo) {
                _path = path;
                _fileInfo = fileInfo;
            }

            #region Implementation of IStorageFile

            public string GetPath() {
                return _path;
            }

            public string GetName() {
                return _fileInfo.Name;
            }

            public long GetSize() {
                return _fileInfo.Length;
            }

            public DateTime GetLastUpdated() {
                return _fileInfo.LastWriteTime;
            }

            public string GetFileType() {
                return _fileInfo.Extension;
            }

            public Stream OpenRead() {
                return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);
            }

            public Stream OpenWrite() {
                return new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite);
            }

            #endregion
        }

        private class FileSystemStorageFolder : IStorageFolder {
            private readonly string _path;
            private readonly DirectoryInfo _directoryInfo;

            public FileSystemStorageFolder(string path, DirectoryInfo directoryInfo) {
                _path = path;
                _directoryInfo = directoryInfo;

                T = NullLocalizer.Instance;
            }

            public Localizer T { get; set; }

            #region Implementation of IStorageFolder

            public string GetPath() {
                return _path;
            }

            public string GetName() {
                return _directoryInfo.Name;
            }

            public DateTime GetLastUpdated() {
                return _directoryInfo.LastWriteTime;
            }

            public long GetSize() {
                return GetDirectorySize(_directoryInfo);
            }

            public IStorageFolder GetParent() {
                if (_directoryInfo.Parent != null) {
                    return new FileSystemStorageFolder(Path.GetDirectoryName(_path), _directoryInfo.Parent);
                }
                throw new ArgumentException(T("Directory {0} does not have a parent directory", _directoryInfo.Name).ToString());
            }

            #endregion

            private static long GetDirectorySize(DirectoryInfo directoryInfo) {
                long size = 0;

                FileInfo[] fileInfos = directoryInfo.GetFiles();
                foreach (FileInfo fileInfo in fileInfos) {
                    if (!IsHidden(fileInfo)) {
                        size += fileInfo.Length;
                    }
                }
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                foreach (DirectoryInfo dInfo in directoryInfos) {
                    if (!IsHidden(dInfo)) {
                        size += GetDirectorySize(dInfo);
                    }
                }

                return size;
            }
        }

    }
}
#endif