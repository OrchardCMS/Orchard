using System;
using Orchard.AuditTrail.Models;

namespace Orchard.AuditTrail.Services.Models {
    public class AuditTrailFilterParameters {
        public Filters Filters { get; set; }
        public string UserName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}