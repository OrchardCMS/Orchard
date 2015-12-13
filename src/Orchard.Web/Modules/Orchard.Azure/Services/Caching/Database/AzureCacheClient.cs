using System;
using System.Globalization;
using Microsoft.ApplicationServer.Caching;
using NHibernate;
using NHibernate.Cache;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheClient : ICache {

        public AzureCacheClient(DataCache cache, string region, TimeSpan? expirationTime) {
            _logger = LoggerProvider.LoggerFor(typeof(AzureCacheClient));
            _cache = cache;
            _region = region ?? DefaultRegion;
            // Azure Cache supports only alphanumeric strings for regions, but
            // NHibernate can get a lot more creative than that. Remove all non
            // alphanumering characters from the region, and append the hash code
            // of the original string to mitigate the risk of two distinct original
            // region strings yielding the same transformed region string.
            _regionAlphaNumeric = new String(Array.FindAll(_region.ToCharArray(), Char.IsLetterOrDigit)) + _region.GetHashCode().ToString(CultureInfo.InvariantCulture);
            _expirationTime = expirationTime;

            _cache.CreateRegion(_regionAlphaNumeric);

            //_lockHandleDictionary = new ConcurrentDictionary<object, DataCacheLockHandle>();
            //_lockTimeout = TimeSpan.FromSeconds(30);

            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("Created an AzureCacheClient for region '{0}' (original region '{1}').", _regionAlphaNumeric, _region);
            }
        }

        private const string DefaultRegion = "NHibernate";
        private readonly IInternalLogger _logger;
        private readonly DataCache _cache;
        private readonly string _region;
        private readonly string _regionAlphaNumeric;
        private readonly TimeSpan? _expirationTime;

        public object Get(object key) {
            if (key == null) {
                throw new ArgumentNullException("key", "The parameter 'key' must not be null.");
            }

            if (_logger.IsDebugEnabled)
                _logger.DebugFormat("Get() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);

            return _cache.Get(key.ToString(), _regionAlphaNumeric);
        }

        public void Put(object key, object value) {
            if (key == null) {
                throw new ArgumentNullException("key", "The parameter 'key' must not be null.");
            }
            if (value == null) {
                throw new ArgumentNullException("value", "The parameter 'value' must not be null.");
            }

            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("Put() invoked with key='{0}' and value='{1}' in region '{2}'.", key, value, _regionAlphaNumeric);
            }

            if (_expirationTime.HasValue) {
                _cache.Put(key.ToString(), value, _expirationTime.Value, _regionAlphaNumeric);
            }
            else {
                _cache.Put(key.ToString(), value, _regionAlphaNumeric);
            }
        }

        public void Remove(object key) {
            if (key == null) {
                throw new ArgumentNullException("key", "The parameter 'key' must not be null.");
            }

            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("Remove() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            }

            _cache.Remove(key.ToString(), _regionAlphaNumeric);
        }

        public void Clear() {
            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("Clear() invoked in region '{0}'.", _regionAlphaNumeric);
            }

            _cache.ClearRegion(_regionAlphaNumeric);
        }

        public void Destroy() {
            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("Destroy() invoked in region '{0}'.", _regionAlphaNumeric);
            }

            Clear();
        }

        // The NHibernate locking mechanism and the Azure Cache pessimistic concurrency
        // model are not a perfect fit. For example, Azure Cache has atomic "get-and-lock" 
        // and "put-and-unlock" semantics but there are no corresponding atomic operations 
        // defined on the ICache interface of NHibernate. Also, Azure Cache does not 
        // strictly enforce the pessimistic concurrency model - clients are responsible
        // for following the locking protocol and therefore the implementation assumes that 
        // NHibernate will always call ICache.Lock() before calling ICache.Put() for data 
        // with concurrency management requirements. The implementations of ICache.Lock() 
        // and ICache.Unlock() below are therefore not as elegant as they would otherwise 
        // be (if not downright hackish).

        // TODO: Try to understand how it's used, and make locking more robust.
        public void Lock(object key) {
            //if (key == null)
            //    throw new ArgumentNullException("key", "The parameter 'key' must not be null.");

            //if (_logger.IsDebugEnabled)
            //    _logger.DebugFormat("Lock() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);

            //try {
            //    DataCacheLockHandle lockHandle = null;
            //    _cache.GetAndLock(key.ToString(), _lockTimeout, out lockHandle, _regionAlphaNumeric);
            //    _lockHandleDictionary.TryAdd(key, lockHandle);
            //}
            //catch (Exception ex) {
            //    if (_logger.IsErrorEnabled)
            //        _logger.Error("Exception thrown while trying to lock object in cache.", ex);

            //    throw;
            //}
        }

        // TODO: Try to understand how it's used, and make locking more robust.
        public void Unlock(object key) {
            //if (key == null)
            //    throw new ArgumentNullException("key", "The parameter 'key' must not be null.");

            //if (_logger.IsDebugEnabled)
            //    _logger.DebugFormat("Unlock() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);

            //try {
            //    DataCacheLockHandle lockHandle = null;
            //    if (_lockHandleDictionary.TryRemove(key, out lockHandle))
            //        _cache.Unlock(key.ToString(), lockHandle, _regionAlphaNumeric);
            //}
            //catch (Exception ex) {
            //    if (_logger.IsErrorEnabled)
            //        _logger.Error("Exception thrown while trying to unlock object in cache.", ex);

            //    throw;
            //}
        }

        // TODO: Try to understand what this is for and how it's used.
        public long NextTimestamp() {
            if (_logger.IsDebugEnabled) {
                _logger.DebugFormat("NextTimestamp() invoked in region '{0}'.", _regionAlphaNumeric);
            }

            return Timestamper.Next();
        }

        // TODO: Try to understand what this is for and how it's used.
        public int Timeout {
            get {
                //return Timestamper.OneMs * (int)_lockTimeout.TotalMilliseconds;
                return Timestamper.OneMs * 60000;
            }
        }

        public string RegionName {
            get {
                // Return original region here (which may be non-alphanumeric) so NHibernate
                // will recognize it as the same region supplied to the constructor.
                return _region;
            }
        }
    }
}