using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Providers.AuditTrail;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Orchard.AuditTrail.Handlers {
    [OrchardFeature("Orchard.AuditTrail.Trimming")]
    public class AuditTrailTrimmingSettingsPartHandler : ContentHandler {
        private int _oldRetentionPeriod;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private int _oldMinimumRunInterval;

        public AuditTrailTrimmingSettingsPartHandler(IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _auditTrailManager = auditTrailManager;
            _wca = wca;
            Filters.Add(new ActivatingFilter<AuditTrailTrimmingSettingsPart>("Site"));
            OnGetContentItemMetadata<AuditTrailTrimmingSettingsPart>(GetMetadata);
            OnUpdating<AuditTrailTrimmingSettingsPart>(BeginUpdateEvent);
            OnUpdated<AuditTrailTrimmingSettingsPart>(EndUpdateEvent);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void GetMetadata(GetContentItemMetadataContext context, AuditTrailTrimmingSettingsPart part) {
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail")));
        }

        private void BeginUpdateEvent(UpdateContentContext context, AuditTrailTrimmingSettingsPart part) {
            _oldRetentionPeriod = part.RetentionPeriod;
            _oldMinimumRunInterval = part.MinimumRunInterval;
        }

        private void EndUpdateEvent(UpdateContentContext context, AuditTrailTrimmingSettingsPart part) {
            var newRetentionPeriod = part.RetentionPeriod;
            var newMinimumRunInterval = part.MinimumRunInterval;

            if (newRetentionPeriod == _oldRetentionPeriod && newMinimumRunInterval == _oldMinimumRunInterval)
                return;

            _auditTrailManager.CreateRecord<AuditTrailTrimmingSettingsEventProvider>(
                eventName: AuditTrailTrimmingSettingsEventProvider.TrimmingSettingsChanged,
                user: _wca.GetContext().CurrentUser,
                eventData: new Dictionary<string, object> {
                    {"OldRetentionPeriod", _oldRetentionPeriod},
                    {"NewRetentionPeriod", newRetentionPeriod},
                    {"OldMinimumRunInterval", _oldMinimumRunInterval},
                    {"NewMinimumRunInterval", newMinimumRunInterval}
                });
        }
    }
}