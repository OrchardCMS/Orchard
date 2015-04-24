using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.AuditTrail.ViewModels {
    public class AuditTrailCategorySettingsViewModel {
        public string Category { get; set; }
        public LocalizedString Name { get; set; }
        public IList<AuditTrailEventSettingsViewModel> Events { get; set; }
    }
}