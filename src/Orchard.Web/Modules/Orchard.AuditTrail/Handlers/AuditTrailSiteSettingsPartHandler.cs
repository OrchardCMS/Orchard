using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.AuditTrail.Handlers {
    public class AuditTrailSiteSettingsPartHandler : ContentHandler {
        public AuditTrailSiteSettingsPartHandler() {
            OnActivated<AuditTrailSiteSettingsPart>(SetupLazyFields);
            OnGetContentItemMetadata<AuditTrailSiteSettingsPart>(GetMetadata);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private void GetMetadata(GetContentItemMetadataContext context, AuditTrailSiteSettingsPart part) {
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Audit Trail")));
        }

        private void SetupLazyFields(ActivatedContentContext context, AuditTrailSiteSettingsPart part) {
            part._eventProviderSettingsField.Loader(() => DeserializeProviderConfiguration(part.Retrieve<string>("Events")));
            part._eventProviderSettingsField.Setter(value => {
                part.Store("Events", SerializeProviderConfiguration(value));
                return value;
            });
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