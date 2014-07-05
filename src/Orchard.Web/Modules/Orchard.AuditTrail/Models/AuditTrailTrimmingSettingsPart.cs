using System;
using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailTrimmingSettingsPart : ContentPart {
        /// <summary>
        /// Gets or sets the retention period in days of audit trail records before they are deleted.
        /// </summary>
        public int RetentionPeriod {
            get { return this.Retrieve(x => x.RetentionPeriod, defaultValue: 10); }
            set { this.Store(x => x.RetentionPeriod, value); }
        }

        /// <summary>
        /// Gets or sets the time in UTC at which the audit trail was last trimmed.
        /// </summary>
        public DateTime? LastRunUtc {
            get { return this.Retrieve(x => x.LastRunUtc); }
            set { this.Store(x => x.LastRunUtc, value); }
        }
    }
}