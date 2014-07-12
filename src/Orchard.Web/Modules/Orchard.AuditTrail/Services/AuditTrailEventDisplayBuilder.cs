using System;
using Orchard.AuditTrail.Models;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailEventDisplayBuilder : IAuditTrailEventDisplayBuilder {
        private readonly IEventDataSerializer _serializer;
        private readonly IAuditTrailManager _auditTrailManager;

        public AuditTrailEventDisplayBuilder(IShapeFactory shapeFactory, IEventDataSerializer serializer, IAuditTrailManager auditTrailManager) {
            _serializer = serializer;
            _auditTrailManager = auditTrailManager;
            New = shapeFactory;
        }

        public dynamic New { get; set; }

        public dynamic BuildDisplay(AuditTrailEventRecord record, string displayType) {
            var eventData = _serializer.Deserialize(record.EventData);
            var descriptor = _auditTrailManager.DescribeEvent(record.FullEventName);
            var auditTrailEventShape = New.AuditTrailEvent(Record: record, EventData: eventData, Descriptor: descriptor);
            var metaData = (ShapeMetadata)auditTrailEventShape.Metadata;
            metaData.DisplayType = displayType;
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}", displayType));
            metaData.Alternates.Add(String.Format("AuditTrailEvent__{0}", record.Category));
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}__{1}", displayType, record.Category));
            metaData.Alternates.Add(String.Format("AuditTrailEvent__{0}__{1}", record.Category, record.EventName));
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}__{1}__{2}", displayType, record.Category, record.EventName));
            return auditTrailEventShape;
        }
    }
}