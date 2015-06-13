using System;
using System.Web.Caching;

namespace IDeliverable.Licensing
{
    public class CacheService
    {
        private readonly HttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

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