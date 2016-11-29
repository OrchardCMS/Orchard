using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Orchard.OpenId {
    public class InMemoryCache : TokenCache {
        private const string CacheIdSuffix = "_TokenCache";
        private static ConcurrentDictionary<string, byte[]> _inMemoryTokenCache;

        private string _userObjectId;
        private string _cacheId;

        public InMemoryCache(string userId) {
            _userObjectId = userId;
            _cacheId = String.Concat(_userObjectId, CacheIdSuffix);

            if (_inMemoryTokenCache == null)
                _inMemoryTokenCache = new ConcurrentDictionary<string, byte[]>();

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;

            Load();
        }

        public override void Clear()
        {
            base.Clear();

            byte[] oldData;
            _inMemoryTokenCache.TryRemove(_cacheId, out oldData);
        }

        private void Load() {
            if (_inMemoryTokenCache.ContainsKey(_cacheId)) {
                byte[] data;
                _inMemoryTokenCache.TryGetValue(_cacheId, out data);

                if (data != default(byte[]))
                    Deserialize(data);
            }
        }

        private void Persist() {
            HasStateChanged = false;

            _inMemoryTokenCache.AddOrUpdate(_cacheId, Serialize(), (key, current) => { return Serialize(); });
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args) {
            Load();
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args) {
            if (HasStateChanged) {
                Persist();
            }
        }
    }
}