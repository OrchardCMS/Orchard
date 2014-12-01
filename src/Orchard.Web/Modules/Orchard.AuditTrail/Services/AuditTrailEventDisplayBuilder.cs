using System;
using System.Collections.Generic;
using Orchard.AuditTrail.Models;
using Orchard.DisplayManagement;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailEventDisplayBuilder : IAuditTrailEventDisplayBuilder {
        private readonly IShapeFactory _shapeFactory;
        private readonly IEventDataSerializer _serializer;
        private readonly IAuditTrailManager _auditTrailManager;

        public AuditTrailEventDisplayBuilder(IShapeFactory shapeFactory, IEventDataSerializer serializer, IAuditTrailManager auditTrailManager) {
            _shapeFactory = shapeFactory;
            _serializer = serializer;
            _auditTrailManager = auditTrailManager;
        }

        public dynamic BuildDisplay(AuditTrailEventRecord record, string displayType) {
            return BuildEventShape("AuditTrailEvent", record, displayType);
        }

        public dynamic BuildActions(AuditTrailEventRecord record, string displayType) {
            return BuildEventShape("AuditTrailEventActions", record, displayType);
        }

        private dynamic BuildEventShape(string shapeType, AuditTrailEventRecord record, string displayType) {
            var eventData = _serializer.Deserialize(record.EventData);
            var descriptor = _auditTrailManager.DescribeEvent(record);
            var auditTrailEventActionsShape = _shapeFactory.Create(shapeType, Arguments.From(new Dictionary<string, object> {
                {"Record", record},
                {"EventData", eventData},
                {"Descriptor", descriptor}
            }));
            var metaData = auditTrailEventActionsShape.Metadata;
            metaData.DisplayType = displayType;
            metaData.Alternates.Add(String.Format("{0}_{1}", shapeType, displayType));
            metaData.Alternates.Add(String.Format("{0}__{1}", shapeType, record.Category));
            metaData.Alternates.Add(String.Format("{0}_{1}__{2}", shapeType, displayType, record.Category));
            metaData.Alternates.Add(String.Format("{0}__{1}__{2}", shapeType, record.Category, record.EventName));
            metaData.Alternates.Add(String.Format("{0}_{1}__{2}__{3}", shapeType, displayType, record.Category, record.EventName));
            return auditTrailEventActionsShape;
        }
    }
}