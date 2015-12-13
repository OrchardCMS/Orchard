namespace Orchard.OutputCache.Models {
    public class CacheParameterRecord {
        public virtual int Id { get; set; }
        public virtual string RouteKey { get; set; }
        public virtual int? Duration { get; set; }
        public virtual int? GraceTime { get; set; }
    }
}