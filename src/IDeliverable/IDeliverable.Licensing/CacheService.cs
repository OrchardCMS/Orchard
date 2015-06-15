using System;
using System.Web.Caching;

namespace IDeliverable.Licensing
{
    public class CacheService
    {
        private readonly HttpContextAccessor mHttpContextAccessor = new HttpContextAccessor();

        private Cache Cache => mHttpContextAccessor.Current().Cache;

        public void SetValue<T>(string key, T value, CacheDependency dependency = null, TimeSpan? validFor = null)
        {
            var absoluteExpiration = validFor != null ? DateTime.UtcNow.Add(validFor.Value) : Cache.NoAbsoluteExpiration;
            Cache.Insert(key, value, dependency, absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public T GetValue<T>(string key, Func<CacheInvalidationContext, T> valueFactory = null)
        {
            var value = Cache.Get(key);
            var exception = value as Exception;

            if (exception != null)
                throw (Exception)value;

            if (value != null)
                return (T)value;

            if (valueFactory == null)
                return default(T);

            var context = new CacheInvalidationContext();

            try
            {
                value = valueFactory(context);
                SetValue(key, value, context.CacheDependency, context.ValidFor);
                return (T) value;
            }
            catch (Exception ex)
            {
                SetValue(key, ex, context.CacheDependency, context.ValidFor);
                throw;
            }
        }

        public object RemoveValue(string key)
        {
            return Cache.Remove(key);
        }
    }
}