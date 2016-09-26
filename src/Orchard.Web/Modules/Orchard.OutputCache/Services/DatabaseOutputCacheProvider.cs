using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.OutputCache.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OutputCache.Services;
using Orchard.Services;

namespace Orchard.OutputCache.Services {
    [OrchardFeature("Orchard.OutputCache.Database")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class DatabaseOutputCacheStorageProvider : IOutputCacheStorageProvider {
        private readonly IRepository<CacheItemRecord> _repository;
        private readonly IClock _clock;

        public DatabaseOutputCacheStorageProvider(IRepository<CacheItemRecord> repository, IClock clock) {
            _repository = repository;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
 
        public void Set(string key, CacheItem cacheItem) {
            lock (String.Intern(key)) {
                var records = _repository.Table.Where(x => x.CacheKey == key).ToList();
                var record = records.FirstOrDefault();
                    
                if (record == null) {
                    record = new CacheItemRecord();
                    Convert(cacheItem, record);
                    _repository.Create(record);
                    return;
                }

                Convert(cacheItem, record);
            }
        }

        private void Convert(CacheItem cacheItem, CacheItemRecord record) {
            record.CacheKey = cacheItem.CacheKey;
            record.CachedOnUtc = cacheItem.CachedOnUtc;
            record.Duration = cacheItem.Duration;
            record.GraceTime = cacheItem.GraceTime;
            record.ValidUntilUtc = cacheItem.ValidUntilUtc;
            record.StoredUntilUtc = cacheItem.StoredUntilUtc;
            record.ContentType = cacheItem.ContentType;
            record.InvariantCacheKey = cacheItem.InvariantCacheKey;
            record.Output = cacheItem.Output;
            record.QueryString = cacheItem.QueryString;
            record.StatusCode = cacheItem.StatusCode;
            record.Tags = String.Join(";", cacheItem.Tags);
            record.Tenant = cacheItem.Tenant;
            record.Url = cacheItem.Url;
        }

        private CacheItem Convert(CacheItemRecord record) {
            var cacheItem = new CacheItem();

            cacheItem.CacheKey = record.CacheKey;
            cacheItem.CachedOnUtc = record.CachedOnUtc;
            cacheItem.Duration = record.Duration;
            cacheItem.GraceTime = record.GraceTime;
            cacheItem.ContentType = record.ContentType;
            cacheItem.InvariantCacheKey = record.InvariantCacheKey;
            cacheItem.Output = record.Output;
            cacheItem.QueryString = record.QueryString;
            cacheItem.StatusCode = record.StatusCode;
            cacheItem.Tags = record.Tags.Split(';');
            cacheItem.Tenant = record.Tenant;
            cacheItem.Url = record.Url;

            return cacheItem;
        }

        public void Remove(string key) {
            lock (String.Intern(key)) {
                var records = _repository.Table.Where(x => x.CacheKey == key).ToList();
                var record = records.FirstOrDefault();

                if (record != null) {
                    _repository.Delete(record);
                }
            }
        }

        public void RemoveAll() {
            foreach (var record in _repository.Table) {
                _repository.Delete(record);
            }
        }

        public CacheItem GetCacheItem(string key) {
            lock (String.Intern(key)) {
                var records = _repository.Table.Where(x => x.CacheKey == key).ToList();
                var record = records.FirstOrDefault();

                CacheItem cacheItem = null;
                if (record != null) {
                    cacheItem = Convert(record);
                }

                return cacheItem;
            }
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            return _repository.Table
                .OrderByDescending(x => x.CachedOnUtc)
                .Skip(skip)
                .Take(count)
                .Select(Convert)
                .ToList();
        }

        public int GetCacheItemsCount() {
            return _repository.Table.Count();
        }

        public void RemoveExpiredEntries() {
            foreach (var record in _repository.Table.Where( x => x.StoredUntilUtc < _clock.UtcNow)) {
                _repository.Delete(record);
            }
        }
    }
}