using Orchard.AuditTrail.Models;
using Orchard.Core.Common.ViewModels;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailFilterViewModel {
        public AuditTrailFilterViewModel() {
            From = new DateTimeEditor { ShowDate = true, ShowTime = false};
            To = new DateTimeEditor { ShowDate = true, ShowTime = false };
        }
        public string FilterKey { get; set; }
        public string FilterValue { get; set; }
        public string UserName { get; set; }
        public DateTimeEditor From { get; set; }
        public DateTimeEditor To { get; set; }
        public AuditTrailOrderBy OrderBy { get; set; }
    }
}