namespace Orchard.AuditTrail.Models {

    /// <summary>
    /// The created audit trail event result
    /// </summary>
    public class AuditTrailEventRecordResult {

        /// <summary>
        /// The created <see cref="AuditTrailEventRecord"/> 
        /// </summary>
        public AuditTrailEventRecord Record { get; set; }

        /// <summary>
        /// Determines whether AuditTrailEventRecordResult is disabled for <see cref="AuditTrailEventRecord"/> .
        /// </summary>
        public bool IsDisabled { get; set; }
    }
}