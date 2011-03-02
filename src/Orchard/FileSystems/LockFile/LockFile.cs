using System;
using Orchard.FileSystems.AppData;

namespace Orchard.FileSystems.LockFile {
    /// <summary>
    /// Represents a Lock File acquire on the file system
    /// </summary>
    public class LockFile : ILockFile {
        private readonly IAppDataFolder _appDataFolder;
        private readonly string _path;
        private readonly string _content;
        private bool _released;

        public LockFile(IAppDataFolder appDataFolder, string path, string content) {
            _appDataFolder = appDataFolder;
            _path = path;
            _content = content;

            // create the physical lock file
            _appDataFolder.CreateFile(path, content);
        }

        public void Dispose() {
            // dispose both managed and unmanaged resources
            Dispose(true);

            // don't call the finalizer if dispose is called
            GC.SuppressFinalize(this);
        }

        public void Release() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if(disposing) {
                // release managed code here
                // nothing right now, just a placeholder to preserve the pattern
            }

            if (_released || !_appDataFolder.FileExists(_path)) {
                // nothing to do, night happen if re-granted, and already released
                return;
            }

            _released = true;

            // check it has not been granted in the meantime
            var current = _appDataFolder.ReadFile(_path);
            if (current == _content) {
                _appDataFolder.DeleteFile(_path);
            }
        }

        ~LockFile() {
            // dispose unmanaged resources (file)
            Dispose(false);
        }
    }
}
