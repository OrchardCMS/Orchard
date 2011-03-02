using System;
using Orchard.FileSystems.AppData;
using Orchard.Services;

namespace Orchard.FileSystems.LockFile {
    public class DefaultLockFileManager : ILockFileManager {
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;

        public static TimeSpan Expiration { get; private set; }

        public DefaultLockFileManager(IAppDataFolder appDataFolder, IClock clock) {
            _appDataFolder = appDataFolder;
            _clock = clock;
            Expiration = TimeSpan.FromMinutes(10);
        }

        public bool TryAcquireLock(string path, ref ILockFile lockFile) {
            try {
                if(IsLocked(path)) {
                    return false;
                }

                lockFile = new LockFile(_appDataFolder, path, _clock.UtcNow.ToString());
                return true;
            }
            catch {
                // an error occured while reading/creating the lock file
                return false;
            }
        }

        public bool IsLocked(string path) {
            try {
                if (_appDataFolder.FileExists(path)) {
                    var content = _appDataFolder.ReadFile(path);

                    DateTime creationUtc;
                    if (DateTime.TryParse(content, out creationUtc)) {
                        // if expired the file is not removed
                        // it should be automatically as there is a finalizer in LockFile
                        // or the next taker can do it, unless it also fails, again
                        return creationUtc.Add(Expiration) > _clock.UtcNow;
                    }
                }
            }
            catch {
                // an error occured while reading the file
                return true;
            }

            return false;
        }
    }
}
