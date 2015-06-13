using System;
using System.Web.Caching;

namespace IDeliverable.Licensing
{
    public class CacheInvalidationContext
    {
        public CacheDependency CacheDependency { get; set; }
        public TimeSpan? ValidFor { get; set; }
    }
}