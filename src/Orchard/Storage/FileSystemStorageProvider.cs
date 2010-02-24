using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Storage {
    public class FileSystemStorageProvider : IStorageProvider {
        #region Implementation of IStorageProvider

        public IStorageFile GetFile(string path) {
            if (!File.Exists(path)) {
                throw new ArgumentException("File " + path + " does not exist");
            }
            return new FileSystemStorageFile(new FileInfo(path));
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            if (!Directory.Exists(path)) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }

            return new DirectoryInfo(path)
                .GetFiles()
                .Where(fi => !IsHidden(fi))
                .Select<FileInfo, IStorageFile>(fi => new FileSystemStorageFile(fi))
                .ToList();
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {
            if (!Directory.Exists(path)) {
                try {
                    Directory.CreateDirectory(path);
                } 
                catch (Exception ex) {
                    throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}",path,ex));
                }
            }

            return new DirectoryInfo(path)
                .GetDirectories()
                .Where(di => !IsHidden(di))
                .Select<DirectoryInfo, IStorageFolder>(di => new FileSystemStorageFolder(di))
                .ToList();
        }

        private static bool IsHidden(FileSystemInfo di) {
            return (di.Attributes & FileAttributes.Hidden) != 0;
        }

        public void CreateFolder(string path) {
            if (Directory.Exists(path)) {
                throw new ArgumentException("Directory " + path + " already exists");
            }

            Directory.CreateDirectory(path);
        }

        public void DeleteFolder(string path) {
            if (!Directory.Exists(path)) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }

            Directory.Delete(path, true);
        }

        public void RenameFolder(string path, string newPath) {
            if (!Directory.Exists(path)) {
                throw new ArgumentException("Directory " + path + "does not exist");
            }

            if (Directory.Exists(newPath)) {
                throw new ArgumentException("Directory " + newPath + " already exists");
            }

            Directory.Move(path, newPath);
        }

        public IStorageFile CreateFile(string path) {
            if (File.Exists(path)) {
                throw new ArgumentException("File " + path + " already exists");
            }

            var fileInfo = new FileInfo(path);
            fileInfo.Create();
            return new FileSystemStorageFile(fileInfo);
        }

        public void DeleteFile(string path) {
            if (!File.Exists(path)) {
                throw new ArgumentException("File " + path + " does not exist");
            }

            File.Delete(path);
        }

        public void RenameFile(string path, string newPath) {
            if (!File.Exists(path)) {
                throw new ArgumentException("File " + path + "does not exist");
            }

            if (File.Exists(newPath)) {
                throw new ArgumentException("File " + newPath + " already exists");
            }

            File.Move(path, newPath);
        }

        #endregion

        private class FileSystemStorageFile : IStorageFile {
            private readonly FileInfo _fileInfo;

            public FileSystemStorageFile(FileInfo fileInfo) {
                _fileInfo = fileInfo;
            }

            #region Implementation of IStorageFile

            public string GetPath() {
                return _fileInfo.FullName;
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

            public Stream OpenStream() {
                return new FileStream(_fileInfo.FullName, FileMode.Open);
            }

            #endregion
        }

        private class FileSystemStorageFolder : IStorageFolder {
            private readonly DirectoryInfo _directoryInfo;

            public FileSystemStorageFolder(DirectoryInfo directoryInfo) {
                _directoryInfo = directoryInfo;
            }

            #region Implementation of IStorageFolder

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
                    return new FileSystemStorageFolder(_directoryInfo.Parent);
                }
                throw new ArgumentException("Directory " + _directoryInfo.Name + " does not have a parent directory");
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
