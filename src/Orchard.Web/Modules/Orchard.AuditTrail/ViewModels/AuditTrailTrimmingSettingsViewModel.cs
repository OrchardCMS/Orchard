using System;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailTrimmingSettingsViewModel {
        public int RetentionPeriod { get; set; }
        public DateTime? LastRun { get; set; }
    }
}