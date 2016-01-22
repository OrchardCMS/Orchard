using System;
using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Orchard.OutputCache.Models {
    [OrchardFeature("Orchard.OutputCache.Database")]
    public class CacheItemRecord {
        public virtual int Id { get; set; }
        public virtual DateTime CachedOnUtc { get; set; }
        public virtual int Duration { get; set; }
        public virtual int GraceTime { get; set; }
        public virtual DateTime ValidUntilUtc { get; set; }
        public virtual DateTime StoredUntilUtc { get; set; }
        [StringLengthMax] public virtual byte[] Output { get; set; }
        public virtual string ContentType { get; set; }
        [StringLength(2048)] public virtual string QueryString { get; set; }
        [StringLength(2048)] public virtual string CacheKey { get; set; }
        [StringLength(2048)] public virtual string InvariantCacheKey { get; set; }
        [StringLength(2048)] public virtual string Url { get; set; }
        public virtual string Tenant { get; set; }
        public virtual int StatusCode { get; set; }
        [StringLengthMax] public virtual string Tags { get; set; }
    }
}
