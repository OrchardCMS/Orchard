using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailEventDescriptorSettingViewModel {
        public AuditTrailEventDescriptor Descriptor { get; set; }
        public AuditTrailEventSetting Setting { get; set; }
    }
}