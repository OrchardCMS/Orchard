using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.ViewModels;
using Orchard;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Utility.Extensions;

namespace Orchard.OutputCache.Services {
    public class CacheService : ICacheService {
        private const string RouteConfigsCacheKey = "OutputCache_RouteConfigs";
        
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<CacheParameterRecord> _repository;
        private readonly ICacheManager _cacheManager;
        private readonly IOutputCacheStorageProvider _cacheStorageProvider;
        private readonly ITagCache _tagCache;
        private readonly ISignals _signals;

        public CacheService(
            IWorkContextAccessor workContextAccessor,
            IRepository<CacheParameterRecord> repository,
            ICacheManager cacheManager,
            IOutputCacheStorageProvider cacheStorageProvider,
            ITagCache tagCache,
            ISignals signals) {
            _workContextAccessor = workContextAccessor;
            _repository = repository;
            _cacheManager = cacheManager;
            _cacheStorageProvider = cacheStorageProvider;
            _tagCache = tagCache;
            _signals = signals;
        }

        public void RemoveByTag(string tag) {
            foreach(var key in _tagCache.GetTaggedItems(tag)) {
                _cacheStorageProvider.Remove(key);
            }

            // we no longer need the tag entry as the items have been removed
            _tagCache.RemoveTag(tag);
        }

        public IEnumerable<CacheItem> GetCacheItems() {
            var workContext = _workContextAccessor.GetContext();

            foreach (DictionaryEntry cacheEntry in workContext.HttpContext.Cache) {
                var cacheItem = cacheEntry.Value as CacheItem;
                if (cacheItem != null) {
                    yield return cacheItem;
                }
            }
        }

        public void Evict(string cacheKey) {
            var workContext = _workContextAccessor.GetContext();
            workContext.HttpContext.Cache.Remove(cacheKey);
        }

        public string GetRouteDescriptorKey(HttpContextBase httpContext, RouteBase routeBase) {
            var route = routeBase as Route;
            var dataTokens = new RouteValueDictionary();

            if (route != null) {
                dataTokens = route.DataTokens;
            }
            else {
            var routeData = routeBase.GetRouteData(httpContext);

                if (routeData != null) {
                    dataTokens = routeData.DataTokens;
                }
            }

            var keyBuilder = new StringBuilder();

            if (route != null) {
                keyBuilder.AppendFormat("url={0};", route.Url);
            }

            // the data tokens are used in case the same url is used by several features, like *{path} (Rewrite Rules and Home Page Provider)
            if (dataTokens != null) {
                foreach (var key in dataTokens.Keys) {
                    keyBuilder.AppendFormat("{0}={1};", key, dataTokens[key]);
                }
            }

            return keyBuilder.ToString().ToLowerInvariant();
        }

        public CacheParameterRecord GetCacheParameterByKey(string key) {
            return _repository.Get(c => c.RouteKey == key);
        }

        public IEnumerable<CacheRouteConfig> GetRouteConfigs() {
            return _cacheManager.Get(RouteConfigsCacheKey, true,
                ctx => {
                    ctx.Monitor(_signals.When(RouteConfigsCacheKey));
                    return _repository.Fetch(c => true).Select(c => new CacheRouteConfig { RouteKey = c.RouteKey, Duration = c.Duration, GraceTime = c.GraceTime }).ToReadOnlyCollection();
                });
        }

        public void SaveRouteConfigs(IEnumerable<CacheRouteConfig> routeConfigurations) {
            // remove all current configurations
            var configurations = _repository.Fetch(c => true);
            foreach (var configuration in configurations) {
                _repository.Delete(configuration);
            }

            // save the new configurations
            foreach (var configuration in routeConfigurations) {
                if (!configuration.Duration.HasValue && !configuration.GraceTime.HasValue) {
                    continue;
                }

                _repository.Create(new CacheParameterRecord {
                    Duration = configuration.Duration,
                    GraceTime = configuration.GraceTime,
                    RouteKey = configuration.RouteKey
                });
            }

            // invalidate the cache
            _signals.Trigger(RouteConfigsCacheKey);
        }
    }
}