using System;
using System.Text;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.Locking {
    /// <summary>
    /// Implements <seealso cref="ILockManager"/> by creating lock files in App_Data/Sites/[Tenant].
    /// A lock file is implemented by  <seealso cref="FileLock"/>. When a lock is requested, if the lock file
    /// doesn't exist, it is created, and returned. Otherwise, the file is read, and its timestamp is analyzed. If the 
    /// timestamp is over, then the file is deleted and the lock is granted. Otherwise the lock is not granted.
    /// This implementation is thread safe so that multiple process can try to acquire a lock on the same
    /// resource at the same time. 
    /// This class should not be used directly. Use <seealso cref="ILockManager"/> instead.
    /// </summary>
    public class DefaultLockManager : ILockManager {
        private readonly IAppDataFolder _appDataFolder;
        private readonly IClock _clock;
        public static TimeSpan Expiration { get; private set; }
        private readonly string _tenantPrefix = String.Empty;
        private readonly object _synLock = new object();

        public ILogger Logger { get; set; }

        static DefaultLockManager() {
            // default maximum time a lock file can be acquired
            Expiration = TimeSpan.FromMinutes(10);
        }

        public DefaultLockManager(
            IAppDataFolder appDataFolder, 
            IClock clock,
            ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _clock = clock;

            _tenantPrefix = appDataFolder.Combine("Sites", shellSettings.Name);
        }

        public IDisposable Lock(string resourceKey) {
            var filename = GetFilenameFromResourceKey(resourceKey);

            try {
                // lock to prevent concurrent tenant's requests
                lock (_synLock) {
                    if (IsLocked(filename)) {
                        return null;
                    }

                    var path = _appDataFolder.Combine(_tenantPrefix, filename);

                    return new FileLock(_appDataFolder, path, _clock.UtcNow.ToString(), _synLock);
                }
            }
            catch (Exception e) {
                // an error occured while reading/creating the lock file
                Logger.Error(e, "Unexpected error while locking {0}", resourceKey);
                return null;
            }
        }

        private bool IsLocked(string filename) {
            var path = _appDataFolder.Combine(_tenantPrefix, filename);
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

            return false;
        }

        private static string GetFilenameFromResourceKey(string resourceKey) {
            if (String.IsNullOrWhiteSpace(resourceKey)) {
                throw new ArgumentException("resourceKey can't be empty");
            }

            var sb = new StringBuilder();
            foreach (var c in resourceKey) {
                // only accept alphanumeric chars
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) {
                    sb.Append(c);
                }
                // otherwise encode them in UTF8
                else {
                    sb.Append("_");
                    foreach(var b in Encoding.UTF8.GetBytes(new [] {c})) {
                        sb.Append(b.ToString("X"));
                    }
                }
            }

            // give an extension to the filename for semantic
            sb.Append(".lock");

            return sb.ToString();
        }
    }
}
