using System;
using System.Collections.Concurrent;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Orchard.Environment.Extensions;

namespace Orchard.OpenId.Services.AzureActiveDirectory {
    [OrchardFeature("Orchard.OpenId.AzureActiveDirectory")]
    public class InMemoryCache : TokenCache, ISingletonDependency {
        public InMemoryCache() {
            _inMemoryTokenCache = new ConcurrentDictionary<string, byte[]>();

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;

            Load();
        }

        private const string CacheIdSuffix = "_TokenCache";
        private static ConcurrentDictionary<string, byte[]> _inMemoryTokenCache;
        private string _cacheId;
        private string _userObjectId;

        public string UserObjectId {
            get {
                return _userObjectId;
            }
            set {
                _userObjectId = value;
                _cacheId = String.Concat(_userObjectId, CacheIdSuffix);
            }
        }

        public override void Clear() {
            base.Clear();

            if (String.IsNullOrWhiteSpace(_cacheId))
                return;

            byte[] oldData;
            _inMemoryTokenCache.TryRemove(_cacheId, out oldData);
        }

        private void Load() {
            if (String.IsNullOrWhiteSpace(_cacheId))
                return;

            if (_inMemoryTokenCache.ContainsKey(_cacheId)) {
                byte[] data;
                _inMemoryTokenCache.TryGetValue(_cacheId, out data);

                if (data != default(byte[]))
                    Deserialize(data);
            }
        }

        private void Persist() {
            if (String.IsNullOrWhiteSpace(_cacheId))
                return;

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