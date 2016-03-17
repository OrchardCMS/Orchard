using System;

namespace Orchard.OutputCache.Models {
    [Serializable]
    public class CacheItem {
        // used for serialization compatibility
        public static readonly string Version = "2";

        public DateTime CachedOnUtc { get; set; }
        public int Duration { get; set; }
        public int GraceTime { get; set; }
        public byte[] Output { get; set; }
        public string ContentType { get; set; }
        public string QueryString { get; set; }
        public string CacheKey { get; set; }
        public string InvariantCacheKey { get; set; }
        public string Url { get; set; }
        public string Tenant { get; set; }
        public int StatusCode { get; set; }
        public string[] Tags { get; set; }
        public string ETag { get; set; }

        public int ValidFor {
            get { return Duration; }
        }

        public DateTime ValidUntilUtc {
            get { return CachedOnUtc.AddSeconds(ValidFor); }
        }

        public bool IsValid(DateTime utcNow) {
            return utcNow < ValidUntilUtc;
        }

        public int StoredFor {
            get { return Duration + GraceTime; }
        }

        public DateTime StoredUntilUtc {
            get { return CachedOnUtc.AddSeconds(StoredFor); }
        }

        public bool IsInGracePeriod(DateTime utcNow) {
            return utcNow > ValidUntilUtc && utcNow < StoredUntilUtc;
        }
    }
}