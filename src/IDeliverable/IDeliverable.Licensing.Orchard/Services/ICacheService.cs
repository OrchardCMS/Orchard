using System;
using System.Web.Caching;
using IDeliverable.Licensing.Orchard.Models;

namespace IDeliverable.Licensing.Orchard.Services
{
    public interface ICacheService
    {
        void SetValue<T>(string key, T value, CacheDependency dependency = null, TimeSpan? validFor = null);
        T GetValue<T>(string key, Func<CacheInvalidationContext, T> valueFactory = null);
    }
}