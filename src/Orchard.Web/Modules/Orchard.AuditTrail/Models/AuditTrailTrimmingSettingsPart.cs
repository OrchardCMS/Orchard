using System;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailTrimmingSettingsPart : ContentPart {
        /// <summary>
        /// Threshold in days.
        /// </summary>
        public int Threshold {
            get { return this.Retrieve(x => x.Threshold, defaultValue: 10); }
            set { this.Store(x => x.Threshold, value); }
        }

        /// <summary>
        /// The timestamp the audit trail was last trimmed.
        /// </summary>
        public DateTime? LastRunUtc {
            get { return this.Retrieve(x => x.LastRunUtc); }
            set { this.Store(x => x.LastRunUtc, value); }
        }
    }
}