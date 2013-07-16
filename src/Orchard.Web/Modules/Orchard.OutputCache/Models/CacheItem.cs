using System;

namespace Orchard.OutputCache.Models {
    [Serializable]
    public class CacheItem {
        public int ValidFor { get; set; }
        public DateTime CachedOnUtc { get; set; }
        public string Output { get; set; }
        public string ContentType { get; set; }
        public string QueryString { get; set; }
        public string CacheKey { get; set; }
        public string InvariantCacheKey { get; set; }
        public string Url { get; set; }
        public string Tenant { get; set; }
        public int StatusCode { get; set; }
        public string[] Tags { get; set; }

        public DateTime ValidUntilUtc {
            get { return CachedOnUtc.AddSeconds(ValidFor); }
        }
    }
}