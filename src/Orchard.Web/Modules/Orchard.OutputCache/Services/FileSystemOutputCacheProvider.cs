using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.OutputCache.Models;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Services;
using Orchard.FileSystems.AppData;
using Orchard.Environment.Configuration;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Orchard.OutputCache.Services {
    [OrchardFeature("Orchard.OutputCache.FileSystem")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    /// <summary>
    /// This class provides an implementation of <see cref="IOutputCacheStorageProvider"/>
    /// based on the local App_Data folder, inside <c>OuputCache/{tenant}</c>. It is not 
    /// recommended when used in a server farm.
    /// The <see cref="CacheItem"/> instances are binary serialized.
    /// </summary>
    /// <remarks>
    /// This provider doesn't implement quotas support yet.
    /// </remarks>
    public class FileSystemOutputCacheStorageProvider : IOutputCacheStorageProvider {
        private readonly IClock _clock;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly string _root;

        public FileSystemOutputCacheStorageProvider(IClock clock, IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _clock = clock;
            _shellSettings = shellSettings;
            _root = _appDataFolder.Combine("OutputCache", _shellSettings.Name);

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
 
        public void Set(string key, CacheItem cacheItem) {
            Retry(() => {
                if (cacheItem == null) {
                    throw new ArgumentNullException("cacheItem");
                }

                if (cacheItem.ValidFor <= 0) {
                    return;
                }

                var filename = GetCacheItemFilename(key);

                using (var stream = Serialize(cacheItem)) {
                    using (var fileStream = _appDataFolder.CreateFile(filename)) {
                        stream.CopyTo(fileStream);
                    }
                }
            });
        }

        public void Remove(string key) {
            Retry(() => {
                var filename = GetCacheItemFilename(key);
                if (_appDataFolder.FileExists(filename)) {
                    _appDataFolder.DeleteFile(filename);
                }
            });
        }

        public void RemoveAll() {
            foreach(var filename in _appDataFolder.ListFiles(_root)) {
                if(_appDataFolder.FileExists(filename)) {
                    _appDataFolder.DeleteFile(filename);
                }
            }
        }

        public CacheItem GetCacheItem(string key) {
            return Retry(() => {
                var filename = GetCacheItemFilename(key);

                if (!_appDataFolder.FileExists(filename)) {
                    return null;
                }

                using (var stream = _appDataFolder.OpenFile(filename)) {

                    if (stream == null) {
                        return null;
                    }

                    return Deserialize(stream);
                }
            });
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            return _appDataFolder.ListFiles(_root)
                .OrderBy(x => x)
                .Skip(skip)
                .Take(count)
                .Select(filename => {
                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        return Deserialize(stream);
                    }
                })
                .ToList();
        }

        public int GetCacheItemsCount() {
            return _appDataFolder.ListFiles(_root).Count();
        }
        
        private string GetCacheItemFilename(string key) {
            return _appDataFolder.Combine(_root, HttpUtility.UrlEncode(key));
        }

        internal static MemoryStream Serialize(CacheItem item) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, item);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        internal static CacheItem Deserialize(Stream stream) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var result = (CacheItem)binaryFormatter.Deserialize(stream);
            return result;
        }

        private T Retry<T>(Func<T> action) {
            var retries = 3;
            for (int i = 1; i <= retries; i++) {
                try {
                    var t = action();
                    return t;
                }
                catch {
                    if (i == retries) {
                        throw;
                    }
                }
            }

            return default(T);
        }

        private void Retry(Action action) {
            var retries = 3;
            for(int i=1; i <= retries; i++) {
                try {
                    action();
                }
                catch {
                    if(i == retries) {
                        throw;
                    }
                }
            }
        }
    }
}