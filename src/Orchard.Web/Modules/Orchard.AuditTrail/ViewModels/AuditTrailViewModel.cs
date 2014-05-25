using System.Collections.Generic;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailViewModel {
        public AuditTrailViewModel() {
            Filter = new AuditTrailFilterViewModel();
        }

        public AuditTrailFilterViewModel Filter { get; set; }
        public IEnumerable<AuditTrailEventSummaryViewModel> Records { get; set; }
        public dynamic List { get; set; }
        public dynamic Pager { get; set; }
    }
}