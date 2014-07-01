using Orchard.Core.Common.ViewModels;

namespace Orchard.AuditTrail.ViewModels {
    public class CommonAuditTrailFilterViewModel {
        public CommonAuditTrailFilterViewModel() {
            From = new DateTimeEditor { ShowDate = true, ShowTime = false};
            To = new DateTimeEditor { ShowDate = true, ShowTime = false };
        }

        public string UserName { get; set; }
        public DateTimeEditor From { get; set; }
        public DateTimeEditor To { get; set; }
    }
}