using System;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailFilterParameters {
        public string FilterKey { get; set; }
        public string FilterValue { get; set; }
        public string UserName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}