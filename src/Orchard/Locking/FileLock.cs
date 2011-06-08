using System;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace Orchard.Locking {
    /// <summary>
    /// Represents a Lock File acquired on the file system
    /// </summary>
    public class FileLock : IDisposable {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _path;
        private readonly string _content;
        private bool _released;

        public ILogger Logger { get; set; }

        public FileLock(IAppDataFolder appDataFolder, string path, string content) {
            _appDataFolder = appDataFolder;
            _path = path;
            _content = content;

            // create the physical lock file
            _appDataFolder.CreateFile(path, content);
        }

        public void Dispose() {
            try {
                lock (_appDataFolder) {
                    if (_released || !_appDataFolder.FileExists(_path)) {
                        // nothing to do, might happen if re-granted, and already released
                        return;
                    }

                    _released = true;

                    // check it has not been granted in the meantime
                    var current = _appDataFolder.ReadFile(_path);
                    if (current == _content) {
                        _appDataFolder.DeleteFile(_path);
                    }
                }
            }
            catch (Exception e) {
                Logger.Error(e, "Unexpected error while release lock {0}", _path);
            }
        }
    }
}
