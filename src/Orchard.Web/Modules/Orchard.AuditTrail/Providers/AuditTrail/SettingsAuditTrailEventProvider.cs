using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;

namespace Orchard.AuditTrail.Providers.AuditTrail {
    public class SettingsAuditTrailEventProvider : AuditTrailEventProviderBase {
        
        public const string EventsChanged = "EventsChanged";

        public override void Describe(DescribeContext context) {
            context.For("AuditTrailSettings", T("Audit Trail Settings"))
                .Event(this, EventsChanged, T("Events changed"), T("Audit trail event settings were changed."), enableByDefault: true, isMandatory: true);
        }
    }
}