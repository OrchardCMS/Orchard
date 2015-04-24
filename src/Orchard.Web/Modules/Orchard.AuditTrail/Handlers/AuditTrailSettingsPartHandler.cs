using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Providers.AuditTrail;
using Orchard.AuditTrail.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.AuditTrail.Handlers {
    public class AuditTrailSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;
        private string _oldEventSettings;

        public AuditTrailSettingsPartHandler(ISignals signals, IAuditTrailManager auditTrailManager, IWorkContextAccessor wca) {
            _signals = signals;
            _auditTrailManager = auditTrailManager;
            _wca = wca;
            Filters.Add(new ActivatingFilter<AuditTrailSettingsPart>("Site"));
            OnActivated<AuditTrailSettingsPart>(SetupLazyFields);
            OnUpdating<AuditTrailSettingsPart>(BeginUpdateEvent);
            OnUpdated<AuditTrailSettingsPart>(EndUpdateEvent);
            OnGetContentItemMetadata<AuditTrailSettingsPart>(GetMetadata);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void GetMetadata(GetContentItemMetadataContext context, AuditTrailSettingsPart part) {
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail")));
        }

        private void SetupLazyFields(ActivatedContentContext context, AuditTrailSettingsPart part) {
            part._eventProviderSettingsField.Loader(() => _auditTrailManager.DeserializeProviderConfiguration(part.Retrieve<string>("Events")));
            part._eventProviderSettingsField.Setter(value => {
                part.Store("Events", _auditTrailManager.SerializeProviderConfiguration(value));
                _signals.Trigger("AuditTrail.EventSettings");
                return value;
            });
        }

        private void BeginUpdateEvent(UpdateContentContext context, AuditTrailSettingsPart part) {
            _oldEventSettings = part.Retrieve<string>("Events");
        }

        private void EndUpdateEvent(UpdateContentContext context, AuditTrailSettingsPart part) {
            var newEventSettings = part.Retrieve<string>("Events");

            if (newEventSettings == _oldEventSettings)
                return;

            _auditTrailManager.CreateRecord<AuditTrailSettingsEventProvider>(
                eventName: AuditTrailSettingsEventProvider.EventsChanged,
                eventData: new Dictionary<string, object> {
                    {"OldSettings", _auditTrailManager.ToEventData(_oldEventSettings)},
                    {"NewSettings", _auditTrailManager.ToEventData(newEventSettings)}
                },
                user: _wca.GetContext().CurrentUser);
        }
    }
}