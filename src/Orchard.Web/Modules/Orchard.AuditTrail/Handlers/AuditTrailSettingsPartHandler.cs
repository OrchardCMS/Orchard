using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Providers.AuditTrail;
using Orchard.AuditTrail.Services;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.AuditTrail.Handlers {
    public class AuditTrailSettingsPartHandler : ContentHandler {
        private readonly ISignals _signals;
        private string _oldEventSettings;
        private readonly IAuditTrailManager _auditTrailManager;
        private readonly IWorkContextAccessor _wca;

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
            part._eventProviderSettingsField.Loader(() => DeserializeProviderConfiguration(part.Retrieve<string>("Events")));
            part._eventProviderSettingsField.Setter(value => {
                part.Store("Events", SerializeProviderConfiguration(value));
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

            _auditTrailManager.CreateRecord<SettingsAuditTrailEventProvider>(
                eventName: SettingsAuditTrailEventProvider.EventsChanged,
                user: _wca.GetContext().CurrentUser);
        }


        private IEnumerable<AuditTrailEventSetting> DeserializeProviderConfiguration(string data) {
            if (String.IsNullOrWhiteSpace(data))
                return Enumerable.Empty<AuditTrailEventSetting>();

            try {
                var doc = XDocument.Parse(data);
                return doc.Element("Events").Elements("Event").Select(x => new AuditTrailEventSetting {
                    EventName = x.Attr<string>("Name"),
                    IsEnabled = x.Attr<bool>("IsEnabled")
                }).ToArray();

            }
            catch (Exception ex) {
                Logger.Error(ex, "Error occurred during deserialization of audit trail settings.");
            }
            return Enumerable.Empty<AuditTrailEventSetting>();
        }

        private string SerializeProviderConfiguration(IEnumerable<AuditTrailEventSetting> settings) {
            var doc = new XDocument(
                new XElement("Events",
                    settings.Select(x => 
                        new XElement("Event", 
                            new XAttribute("Name", x.EventName), 
                            new XAttribute("IsEnabled", x.IsEnabled)))));

            return doc.ToString(SaveOptions.DisableFormatting);
        }
    }
}