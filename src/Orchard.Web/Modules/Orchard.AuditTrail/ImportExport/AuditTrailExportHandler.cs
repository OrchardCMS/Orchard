using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.AuditTrail.Models;
using Orchard.Data;
using Orchard.Events;

namespace Orchard.AuditTrail.ImportExport {
    public interface IExportEventHandler : IEventHandler {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class AuditTrailExportEventHandler : IExportEventHandler {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailEventRepository;

        public AuditTrailExportEventHandler(IRepository<AuditTrailEventRecord> auditTrailEventRepository) {
            _auditTrailEventRepository = auditTrailEventRepository;
        }

        public void Exporting(dynamic context) {
        }

        public void Exported(dynamic context) {

            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains("AuditTrail")) {
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
                    new XAttribute("Name", record.Event),
                    new XAttribute("Category", record.Category),
                    new XAttribute("User", record.UserName),
                    new XAttribute("CreatedUtc", record.CreatedUtc),
                    new XAttribute("EventFilterKey", record.EventFilterKey),
                    new XAttribute("EventFilterData", record.EventFilterData),
                    CreateElement("Comment", record.Comment),
                    CreateCDataElement("EventData", record.EventData)));
            }
        }

        private static XElement CreateElement(string name, string value) {
            return !String.IsNullOrWhiteSpace(value) ? new XElement(name, value) : null;
        }

        private static XElement CreateCDataElement(string name, string value) {
            return !String.IsNullOrWhiteSpace(value) ? new XElement(name, new XCData(value)) : null;
        }
    }
}

