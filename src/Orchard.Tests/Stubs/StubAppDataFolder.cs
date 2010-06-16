using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.Services;

namespace Orchard.Tests.Stubs {
    public class StubAppDataFolder : IAppDataFolder {
        private readonly IClock _clock;
        private readonly StubFileSystem _stubFileSystem;

        public StubAppDataFolder(IClock clock) {
            _clock = clock;
            _stubFileSystem = new StubFileSystem(_clock);
        }

        public StubFileSystem StubFileSystem {
            get { return _stubFileSystem; }
        }

        public IEnumerable<string> ListFiles(string path) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListDirectories(string path) {
            throw new NotImplementedException();
        }

        public bool FileExists(string path) {
            return _stubFileSystem.GetFileEntry(path) != null;
        }

        public string Combine(params string[] paths) {
            return Path.Combine(paths).Replace(Path.DirectorySeparatorChar, '/');
        }

        public void CreateFile(string path, string content) {
            throw new NotImplementedException();
        }

        public Stream CreateFile(string path) {
            return _stubFileSystem.CreateFile(path);
        }

        public string ReadFile(string path) {
            throw new NotImplementedException();
        }

        public Stream OpenFile(string path) {
            return _stubFileSystem.OpenFile(path);
        }

        public void DeleteFile(string path) {
            throw new NotImplementedException();
        }

        public void CreateDirectory(string path) {
            var entry = _stubFileSystem.CreateDirectoryEntry(path);
        }

        public IVolatileToken WhenPathChanges(string path) {
            return _stubFileSystem.WhenPathChanges(path);
        }

        public string MapPath(string path) {
            throw new NotImplementedException();
        }

        public DateTime GetLastWriteTimeUtc(string path) {
            var entry = _stubFileSystem.GetFileEntry(path);
            if (entry == null)
                throw new InvalidOperationException();
            return entry.LastWriteTime;
        }
    }
}