using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Orchard.OpenId {
    public class InMemoryCache : TokenCache {
        private static ConcurrentDictionary<string, byte[]> _inMemoryTokenCache;

        private string _userObjectId;
        private string _cacheId;

        public InMemoryCache(string userId) {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;

            Load();
        }

        public static ConcurrentDictionary<string, byte[]> InMemoryTokenCache {
            get {
                if (_inMemoryTokenCache == null)
                    _inMemoryTokenCache = new ConcurrentDictionary<string, byte[]>();

                return _inMemoryTokenCache;
            }
        }

        public void Load() {
            if (InMemoryTokenCache.ContainsKey(_cacheId))
                Deserialize(InMemoryTokenCache.Where(x => x.Key == _cacheId).First().Value);
        }

        public void Persist() {
            HasStateChanged = false;

            InMemoryTokenCache.AddOrUpdate(_cacheId, Serialize(), (key, current) => { return Serialize(); });
        }

        public override void Clear() {
            byte[] oldData;
            base.Clear();
            InMemoryTokenCache.TryRemove(_cacheId, out oldData);
        }

        void BeforeAccessNotification(TokenCacheNotificationArgs args) {
            Load();
        }

        void AfterAccessNotification(TokenCacheNotificationArgs args) {
            if (HasStateChanged) {
                Persist();
            }
        }
    }
}