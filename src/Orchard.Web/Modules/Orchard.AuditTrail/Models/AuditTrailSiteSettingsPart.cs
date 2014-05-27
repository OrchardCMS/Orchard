using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailSiteSettingsPart : ContentPart {
        internal LazyField<IEnumerable<AuditTrailEventSetting>> _eventProviderSettingsField = new LazyField<IEnumerable<AuditTrailEventSetting>>();

        public bool AutoTrim {
            get { return this.Retrieve(x => x.AutoTrim, defaultValue: false); }
            set { this.Store(x => x.AutoTrim, value); }
        }

        /// <summary>
        /// Threshold in days.
        /// </summary>
        public int AutoTrimThreshold {
            get { return this.Retrieve(x => x.AutoTrimThreshold, defaultValue: 10); }
            set { this.Store(x => x.AutoTrimThreshold, value); }
        }

        public IEnumerable<AuditTrailEventSetting> EventSettings {
            get { return _eventProviderSettingsField.Value; }
            set { _eventProviderSettingsField.Value = value; }
        }
    }
}