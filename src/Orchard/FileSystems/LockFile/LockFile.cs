using System.Threading;
using Orchard.FileSystems.AppData;

namespace Orchard.FileSystems.LockFile {
    /// <summary>
    /// Represents a Lock File acquired on the file system
    /// </summary>
    /// <remarks>
    /// The instance needs to be disposed in order to release the lock explicitly
    /// </remarks>
    public class LockFile : ILockFile {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _path;
        private readonly string _content;
        private readonly ReaderWriterLockSlim _rwLock;
        private bool _released;

        public LockFile(IAppDataFolder appDataFolder, string path, string content, ReaderWriterLockSlim rwLock) {
            _appDataFolder = appDataFolder;
            _path = path;
            _content = content;
            _rwLock = rwLock;

            // create the physical lock file
            _appDataFolder.CreateFile(path, content);
        }

        public void Dispose() {
            Release();
        }

        public void Release() {
            _rwLock.EnterWriteLock();

            try{
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
            finally {
                _rwLock.ExitWriteLock();
            }
        }
    }
}
