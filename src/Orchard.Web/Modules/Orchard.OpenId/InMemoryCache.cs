using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Orchard.OpenId {
    public class InMemoryCache : TokenCache {
        private static Dictionary<string, byte[]> _inMemoryTokenCache;
        private static ReaderWriterLockSlim _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private string _userObjectId;
        private string _cacheId;

        public InMemoryCache(string userId) {
            _userObjectId = userId;
            _cacheId = _userObjectId + "_TokenCache";

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
            _sessionLock.EnterReadLock();
            try
            {
                if (InMemoryTokenCache.ContainsKey(_cacheId))
                    Deserialize(InMemoryTokenCache.Where(x => x.Key == _cacheId).First().Value);
            }
            catch (Exception) {
                _sessionLock.ExitReadLock();
                throw;
            }
            _sessionLock.ExitReadLock();
        }

        public void Persist() {
            _sessionLock.EnterWriteLock();
            try
            {
                HasStateChanged = false;

                if (InMemoryTokenCache.ContainsKey(_cacheId))
                    InMemoryTokenCache.Remove(_cacheId);

                InMemoryTokenCache.Add(_cacheId, Serialize());
            }
            catch (Exception)
            {
                _sessionLock.ExitReadLock();
                throw;
            }
            _sessionLock.ExitWriteLock();
        }

        public override void Clear() {
            base.Clear();
            InMemoryTokenCache.Remove(_cacheId);
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