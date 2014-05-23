using System;
using Orchard.ContentManagement.Records;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailEventRecord {
        public virtual int Id { get; set; }
        public virtual DateTime CreatedUtc { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Event { get; set; }
        public virtual string Category { get; set; }
        public virtual ContentItemVersionRecord ContentItemVersion { get; set; }
        public virtual string EventData { get; set; }
    }
}