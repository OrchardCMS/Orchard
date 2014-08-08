using System.Collections.Generic;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailViewModel {
        public dynamic FilterDisplay { get; set; }
        public AuditTrailOrderBy OrderBy { get; set; }
        public IEnumerable<AuditTrailEventSummaryViewModel> Records { get; set; }
        public dynamic List { get; set; }
        public dynamic Pager { get; set; }
    }
}