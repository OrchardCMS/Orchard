using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;

namespace Orchard.UI.Resources {
    public class ResourceFileHashProvider : IResourceFileHashProvider {
        private ConcurrentDictionary<string, HashInfo> _hashInfoCache = new ConcurrentDictionary<string, HashInfo>();

        public string GetResourceFileHash(string filePath) {
            if (!File.Exists(filePath))
                throw new ArgumentException(String.Format("File with path '{0}' could not be found.", filePath), "physicalPath");
            var lastWriteTime = File.GetLastWriteTimeUtc(filePath);
            var hashInfo =
                _hashInfoCache.AddOrUpdate(filePath,
                    addValueFactory: (key) => new HashInfo(lastWriteTime, ComputeHash(filePath)),
                    updateValueFactory: (key, oldHashInfo) => oldHashInfo.LastWriteTime >= lastWriteTime ? oldHashInfo : new HashInfo(lastWriteTime, ComputeHash(filePath)));
            return hashInfo.Hash;
        }

        private string ComputeHash(string filePath) {
            using (var md5 = MD5.Create()) {
                using (var fileStream = File.OpenRead(filePath)) {
                    var hashBytes = md5.ComputeHash(fileStream);
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }

        private class HashInfo {
            public HashInfo(DateTime lastWriteTime, string hash) {
                LastWriteTime = lastWriteTime;
                Hash = hash;
            }
            public readonly DateTime LastWriteTime;
            public readonly string Hash;
        }
    }
}
