using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.Services;
using Orchard.Time;

namespace Orchard.Tests.Stubs {
    public class StubAppDataFolder : IAppDataFolder {
        private readonly IClock _clock;
        private readonly StubFileSystem _fileSystem;

        public StubAppDataFolder(IClock clock) {
            _clock = clock;
            _fileSystem = new StubFileSystem(_clock);
        }

        public StubFileSystem FileSystem {
            get { return _fileSystem; }
        }

        public IEnumerable<string> ListFiles(string path) {
            var entry = _fileSystem.GetDirectoryEntry(path);
            if (entry == null)
                throw new ArgumentException();

            return entry.Entries.Where(e => e is StubFileSystem.FileEntry).Select(e => Combine(path, e.Name));
        }

        public IEnumerable<string> ListDirectories(string path) {
            var entry = _fileSystem.GetDirectoryEntry(path);
            if (entry == null)
                throw new ArgumentException();

            return entry.Entries.Where(e => e is StubFileSystem.DirectoryEntry).Select(e => Combine(path, e.Name));
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public void StoreFile(string path, string content) {
            using (var stream = _fileSystem.CreateFile(path)) {
                using (var writer = new StreamWriter(stream)) {
                    writer.Write(content);
                }
            }
        }

        public string ReadFile(string path) {
            try {
                using (var stream = OpenFile(path)) {
                    using (var reader = new StreamReader(stream)) {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (InvalidOperationException) {
                return null;
            }
        }

        public Stream OpenFile(string path) {
            return _fileSystem.OpenFile(path);
        }

        public void DeleteFile(string path) {
            _fileSystem.DeleteFile(path);
        }

        public IVolatileToken WhenPathChanges(string path) {
            return _fileSystem.WhenPathChanges(path);
        }

        public string MapPath(string path) {
            return path;
        }

        public string GetVirtualPath(string path) {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTimeUtc(string path) {
            var entry = _fileSystem.GetFileEntry(path);
            if (entry == null)
                throw new InvalidOperationException();
            return entry.LastWriteTimeUtc;
        }
    }
}