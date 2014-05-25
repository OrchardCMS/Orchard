using System;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailEventRecord {
        public virtual int Id { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Event { get; set; }
        public virtual string Category { get; set; }
        public virtual string EventData { get; set; }
        public virtual string EventFilterKey { get; set; }
        public virtual string EventFilterData { get; set; }
        public virtual string Comment { get; set; }
    }
}