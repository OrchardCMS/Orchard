namespace Orchard.AuditTrail.Models {
    public class AuditTrailEventRecordResult {
        public AuditTrailEventRecord Record { get; set; }
        public bool IsDisabled { get; set; }
    }
}