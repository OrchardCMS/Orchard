using System.Collections;
using System.Collections.Generic;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailSiteSettingsViewModel {
        public bool AutoTrim { get; set; }
        public int AutoTrimThreshold { get; set; }
        public IList<AuditTrailCategorySettingsViewModel> Categories { get; set; }
    }
}