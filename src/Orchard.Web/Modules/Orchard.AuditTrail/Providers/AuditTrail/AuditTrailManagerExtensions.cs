using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Services;
using Orchard.ContentManagement;
using Orchard.Logging;

namespace Orchard.AuditTrail.Providers.AuditTrail {
    public static class AuditTrailManagerExtensions {
        public static string ToEventData(this IAuditTrailManager auditTrailManager, string settingsData) {
            var settings = auditTrailManager.DeserializeProviderConfiguration(settingsData);
            var query =
                from setting in settings
                let descriptor = auditTrailManager.DescribeEvent(setting.EventName)
                select new AuditTrailEventSettingEventData {
                    EventName = setting.EventName,
                    IsEnabled = setting.IsEnabled,
                    EventCategory = descriptor.CategoryDescriptor.Name.TextHint,
                    EventDisplayName = descriptor.Name.TextHint
                };

            return SerializeEventData(query);
        }

        public static string SerializeEventData(IEnumerable<AuditTrailEventSettingEventData> settings) {
            var doc = new XDocument(
                new XElement("Events",
                    settings.Select(x =>
                        new XElement("Event",
                            new XAttribute("Name", x.EventName),
                            new XAttribute("IsEnabled", x.IsEnabled),
                            new XAttribute("DisplayName", x.EventDisplayName),
                            new XAttribute("Category", x.EventCategory)))));

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        public static IEnumerable<AuditTrailEventSettingEventData> DeserializeEventData(string data, ILogger logger) {
            if (String.IsNullOrWhiteSpace(data))
                return Enumerable.Empty<AuditTrailEventSettingEventData>();

            try {
                var doc = XDocument.Parse(data);
                return doc.Element("Events").Elements("Event").Select(x => new AuditTrailEventSettingEventData {
                    EventName = x.Attr<string>("Name"),
                    IsEnabled = x.Attr<bool>("IsEnabled"),
                    EventDisplayName = x.Attr<string>("DisplayName"),
                    EventCategory = x.Attr<string>("Category")
                }).ToArray();

            }
            catch (Exception ex) {
                logger.Error(ex, "Error occurred during deserialization of audit trail settings.");
            }
            return Enumerable.Empty<AuditTrailEventSettingEventData>();
        }
    }
}