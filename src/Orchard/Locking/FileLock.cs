using System;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace Orchard.Locking {
    /// <summary>
    /// This class represents a lock lile acquired on the file system. It contains a
    /// timestamp in order to define when it will timeout, in case it has not been released
    /// for too long.
    /// This class should not be used directly. This is an implentation detail
    /// for <seealso cref="DefaultLockManager"/>.
    /// </summary>
    public class FileLock : IDisposable {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _path;
        private readonly string _content;
        private bool _released;
        private object _synLock;

        public ILogger Logger { get; set; }

        public FileLock(IAppDataFolder appDataFolder, string path, string content, object synLock) {
            _appDataFolder = appDataFolder;
            _path = path;
            _content = content;
            _synLock = synLock;

            // create the physical lock file
            _appDataFolder.CreateFile(path, content);
        }

        public void Dispose() {
            try {
                lock (_synLock) {
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
