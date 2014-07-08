using System;
using System.ComponentModel.DataAnnotations;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailTrimmingSettingsViewModel {
        [Range(0, Int32.MaxValue)] public int RetentionPeriod { get; set; }
        [Range(0, Int32.MaxValue)] public int MinimumRunInterval { get; set; }
        public string LastRunDateString { get; set; }
    }
}