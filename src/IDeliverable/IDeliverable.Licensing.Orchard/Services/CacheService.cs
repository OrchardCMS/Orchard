using System;
using System.Web.Caching;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public class CacheService : ICacheService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CacheService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private Cache Cache => _httpContextAccessor.Current().Cache;

        public void SetValue<T>(string key, T value, CacheDependency dependency = null, TimeSpan? validFor = null)
        {
            var absoluteExpiration = validFor != null ? DateTime.UtcNow.Add(validFor.Value) : Cache.NoAbsoluteExpiration;
            Cache.Insert(key, value, dependency, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public T GetValue<T>(string key, Func<CacheInvalidationContext, T> valueFactory = null)
        {
            var value = (T)Cache.Get(key);

            if (value != null)
                return value;

            if (valueFactory == null)
                return default(T);

            var context = new CacheInvalidationContext();
            value = valueFactory(context);

            SetValue(key, value, context.CacheDependency, context.ValidFor);

            return value;
        }
    }
}