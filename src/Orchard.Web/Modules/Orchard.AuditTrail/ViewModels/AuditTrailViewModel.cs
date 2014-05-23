using System.Collections.Generic;
using Orchard.AuditTrail.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailViewModel {
        public IEnumerable<AuditTrailEventRecord> Records { get; set; }
        public dynamic List { get; set; }
        public dynamic Pager { get; set; }
    }
}