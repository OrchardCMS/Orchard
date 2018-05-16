using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.Services;

namespace Orchard.OutputCache.Services {
    [OrchardFeature("Orchard.OutputCache.FileSystem")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    /// <summary>
    /// This class provides an implementation of <see cref="IOutputCacheStorageProvider"/>
    /// based on the local App_Data folder, inside <c>FileCache/{tenant}</c>. It is not 
    /// recommended when used in a server farm, unless the file system is share (Azure App Services).
    /// The <see cref="CacheItem"/> instances are binary serialized.
    /// </summary>
    /// <remarks>
    /// This provider doesn't support quotas yet.
    /// </remarks>
    public class FileSystemOutputCacheStorageProvider : IOutputCacheStorageProvider {
        private readonly IClock _clock;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ShellSettings _shellSettings;
        private readonly string _metadata;
        private readonly string _content;

        public static char[] InvalidPathChars = { '/', '\\', ':', '*', '?', '>', '<', '|' };

        public FileSystemOutputCacheStorageProvider(IClock clock, IAppDataFolder appDataFolder, ShellSettings shellSettings) {
            _appDataFolder = appDataFolder;
            _clock = clock;
            _shellSettings = shellSettings;

            _metadata = GetMetadataPath(appDataFolder, _shellSettings.Name);
            _content = GetContentPath(appDataFolder, _shellSettings.Name);

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

                var hash = GetCacheItemFileHash(key);

                lock (String.Intern(hash)) {
                    var filename = _appDataFolder.Combine(_content, hash);
                    using (var fileStream = _appDataFolder.CreateFile(filename)) {
                        using (var writer = new BinaryWriter(fileStream)) {
                            fileStream.Write(cacheItem.Output, 0, cacheItem.Output.Length);
                        }
                    }

                    using (var stream = SerializeMetadata(cacheItem)) {
                        filename = _appDataFolder.Combine(_metadata, hash);
                        using (var fileStream = _appDataFolder.CreateFile(filename)) {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            });
        }

        public void Remove(string key) {
            var hash = GetCacheItemFileHash(key);
            lock (String.Intern(hash)) {
                Retry(() => {
                    var filename = _appDataFolder.Combine(_metadata, hash);
                    if (_appDataFolder.FileExists(filename)) {
                        _appDataFolder.DeleteFile(filename);
                    }
                });

                Retry(() => {
                    var filename = _appDataFolder.Combine(_content, hash);
                    if (_appDataFolder.FileExists(filename)) {
                        _appDataFolder.DeleteFile(filename);
                    }
                });
            }
        }

        public void RemoveAll() {
            foreach (var folder in new[] { _metadata, _content }) {
                foreach (var filename in _appDataFolder.ListFiles(folder)) {
                    var hash = Path.GetFileName(filename);
                    lock (String.Intern(hash)) {
                        try {
                            if (_appDataFolder.FileExists(filename)) {
                                _appDataFolder.DeleteFile(filename);
                            }
                        }
                        catch (Exception e) {
                            Logger.Warning(e, "An error occured while deleting the file: {0}", filename);
                        }
                    }
                }
            }
        }

        public CacheItem GetCacheItem(string key) {
            return Retry(() => {
                var hash = GetCacheItemFileHash(key);
                lock (String.Intern(hash)) {
                    var filename = _appDataFolder.Combine(_metadata, hash);

                    if (!_appDataFolder.FileExists(filename)) {
                        return null;
                    }

                    CacheItem cacheItem = null;

                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        if (stream == null) {
                            return null;
                        }

                        cacheItem = DeserializeMetadata(stream);

                        // We compare the requested key and the one stored in the metadata
                        // as there could be key collisions with the hashed filenames.
                        if (!cacheItem.CacheKey.Equals(key)) {
                            return null;
                        }
                    }

                    filename = _appDataFolder.Combine(_content, hash);
                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        if (stream == null) {
                            return null;
                        }

                        using(var ms = new MemoryStream()) {
                            stream.CopyTo(ms);
                            cacheItem.Output = ms.ToArray();
                        }
                    }

                    return cacheItem;
                }
            });
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            return _appDataFolder.ListFiles(_metadata)
                .OrderBy(x => x)
                .Skip(skip)
                .Take(count)
                .Select(filename => {
                    using (var stream = _appDataFolder.OpenFile(filename)) {
                        return DeserializeMetadata(stream);
                    }
                })
                .ToList();
        }

        public int GetCacheItemsCount() {
            return _appDataFolder.ListFiles(_metadata).Count();
        }

        public static string GetMetadataPath(IAppDataFolder appDataFolder, string tenant) {
            return appDataFolder.Combine("FileCache", tenant, "metadata");
        }

        public static string GetContentPath(IAppDataFolder appDataFolder, string tenant) {
            return appDataFolder.Combine("FileCache", tenant, "content");
        }

        private string GetCacheItemFileHash(string key) {
            // The key is typically too long to be useful, so we use a hash
            using (var md5 = MD5.Create()) {
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var hashedBytes = md5.ComputeHash(keyBytes);
                var b64 = Convert.ToBase64String(hashedBytes);
                return String.Join("-", b64.Split(InvalidPathChars, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        internal static MemoryStream SerializeMetadata(CacheItem item) {
            var output = item.Output;
            item.Output = new byte[0];

            try {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, item);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            finally {
                item.Output = output;
            }            
        }

        internal static CacheItem DeserializeMetadata(Stream stream) {
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
                catch(Exception e) {
                    Logger.Warning("An unexpected error occured, attempt # {0}, i", e);

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
                    return;
                }
                catch(Exception e) {
                    Logger.Warning("An unexpected error occured, attempt # {0}, i", e);

                    if (i == retries) {
                        throw;
                    }
                }
            }
        }
    }
}