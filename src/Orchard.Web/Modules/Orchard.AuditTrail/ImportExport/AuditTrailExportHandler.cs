using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Services;

namespace Orchard.AuditTrail.ImportExport {
    [OrchardFeature("Orchard.AuditTrail.ImportExport")]
    public class AuditTrailExportEventHandler : IExportEventHandler {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailEventRepository;

        public AuditTrailExportEventHandler(IRepository<AuditTrailEventRecord> auditTrailEventRepository) {
            _auditTrailEventRepository = auditTrailEventRepository;
        }

        public void Exporting(ExportContext context) {
        }

        public void Exported(ExportContext context) {

            if (!context.ExportOptions.CustomSteps.Contains("AuditTrail")) {
                return;
            }

            var records = _auditTrailEventRepository.Table.ToList();

            if (!records.Any()) {
                return;
            }

            var root = new XElement("AuditTrail");
            context.Document.Element("Orchard").Add(root);

            foreach (var record in records) {
                root.Add(new XElement("Event",
                    CreateAttribute("Name", record.EventName),
                    CreateAttribute("FullName", record.FullEventName),
                    CreateAttribute("Category", record.Category),
                    CreateAttribute("User", record.UserName),
                    CreateAttribute("CreatedUtc", record.CreatedUtc),
                    CreateAttribute("EventFilterKey", record.EventFilterKey),
                    CreateAttribute("EventFilterData", record.EventFilterData),
                    CreateElement("Comment", record.Comment),
                    ParseEventData(record.EventData)));
            }
        }

        private static XElement CreateElement(string name, string value) {
            return !String.IsNullOrWhiteSpace(value) ? new XElement(name, value) : null;
        }

        private static XAttribute CreateAttribute(string name, string value) {
            return !String.IsNullOrWhiteSpace(value) ? new XAttribute(name, value) : null;
        }

        private static XAttribute CreateAttribute(string name, object value) {
            return new XAttribute(name, value);
        }

        private static XElement ParseEventData(string eventData) {
            if(String.IsNullOrWhiteSpace(eventData))
                return new XElement("EventData");

            return XElement.Parse(eventData);
        }
    }
}

