using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailTrimmingSettingsPart : ContentPart {
        /// <summary>
        /// Threshold in days.
        /// </summary>
        public int AutoTrimThreshold {
            get { return this.Retrieve(x => x.AutoTrimThreshold, defaultValue: 10); }
            set { this.Store(x => x.AutoTrimThreshold, value); }
        }
    }
}