using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Orchard.OpenId {
    public class InMemoryCache : TokenCache {
        private static Dictionary<string, byte[]> _inMemoryTokenCache;
        private static ReaderWriterLockSlim SessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private string _UserObjectId;
        private string _CacheId;

        public InMemoryCache(string userId) {
            _UserObjectId = userId;
            _CacheId = _UserObjectId + "_TokenCache";

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;

            Load();
        }

        public static Dictionary<string, byte[]> InMemoryTokenCache {
            get {
                if (_inMemoryTokenCache == null)
                    _inMemoryTokenCache = new Dictionary<string, byte[]>();

                return _inMemoryTokenCache;
            }
        }

        public void Load() {
            SessionLock.EnterReadLock();

            if (InMemoryTokenCache.Any(x => x.Key == _CacheId))
                Deserialize(InMemoryTokenCache.Where(x => x.Key == _CacheId).First().Value);

            SessionLock.ExitReadLock();
        }

        public void Persist() {
            SessionLock.EnterWriteLock();

            HasStateChanged = false;

            if (InMemoryTokenCache.Any(x => x.Key == _CacheId))
                InMemoryTokenCache.Remove(_CacheId);

            InMemoryTokenCache.Add(_CacheId, Serialize());

            SessionLock.ExitWriteLock();
        }

        public override void Clear() {
            base.Clear();
            InMemoryTokenCache.Remove(_CacheId);
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