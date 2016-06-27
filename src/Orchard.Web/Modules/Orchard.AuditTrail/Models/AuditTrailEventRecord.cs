using System;
using Orchard.Data.Conventions;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailEventRecord {
        public virtual int Id { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual string UserName { get; set; }
        public virtual string EventName { get; set; }
        public virtual string FullEventName { get; set; }
        public virtual string Category { get; set; }
        
        [StringLengthMax]
        public virtual string EventData { get; set; }
        public virtual string EventFilterKey { get; set; }
        public virtual string EventFilterData { get; set; }

        [StringLengthMax]
        public virtual string Comment { get; set; }
        public virtual string ClientIpAddress { get; set; }
    }
}