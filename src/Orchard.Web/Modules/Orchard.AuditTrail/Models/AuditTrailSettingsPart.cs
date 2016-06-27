using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailSettingsPart : ContentPart {
        internal LazyField<IEnumerable<AuditTrailEventSetting>> _eventProviderSettingsField = new LazyField<IEnumerable<AuditTrailEventSetting>>();

        public IEnumerable<AuditTrailEventSetting> EventSettings {
            get { return _eventProviderSettingsField.Value; }
            set { _eventProviderSettingsField.Value = value; }
        }

        public bool EnableClientIpAddressLogging {
            get { return this.Retrieve(x => x.EnableClientIpAddressLogging); }
            set { this.Store(x => x.EnableClientIpAddressLogging, value); }
        }
    }
}