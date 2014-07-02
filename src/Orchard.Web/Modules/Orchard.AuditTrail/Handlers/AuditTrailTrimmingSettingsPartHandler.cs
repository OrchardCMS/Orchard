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
        private int _oldThreshold;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

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
            _oldThreshold = part.Threshold;
        }

        private void EndUpdateEvent(UpdateContentContext context, AuditTrailTrimmingSettingsPart part) {
            var newThreshold = part.Threshold;

            if (newThreshold == _oldThreshold)
                return;

            _auditTrailManager.CreateRecord<TrimmingSettingsAuditTrailEventProvider>(
                eventName: TrimmingSettingsAuditTrailEventProvider.TrimmingSettingsChanged,
                user: _wca.GetContext().CurrentUser,
                eventData: new Dictionary<string, object> {
                    {"OldThreshold", _oldThreshold},
                    {"NewThreshold", newThreshold}
                });
        }
    }
}