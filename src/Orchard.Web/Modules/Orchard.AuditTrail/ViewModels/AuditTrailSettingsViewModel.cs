using System.Collections.Generic;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailSettingsViewModel {
        public IList<AuditTrailCategorySettingsViewModel> Categories { get; set; }
        public bool EnableClientIpAddressLogging { get; set; }
    }
}