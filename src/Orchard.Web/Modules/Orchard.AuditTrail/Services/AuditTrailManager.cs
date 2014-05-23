using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Orchard.AuditTrail.Models;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailManager : Component, IAuditTrailManager {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailRepository;
        private readonly IEnumerable<IAuditTrailEventProvider> _providers;
        private readonly IClock _clock;
        private readonly IAuditTrailEventHandler _auditTrailEventHandlers;

        public AuditTrailManager(
            IRepository<AuditTrailEventRecord> auditTrailRepository,
            IEnumerable<IAuditTrailEventProvider> providers, 
            IClock clock, 
            IAuditTrailEventHandler auditTrailEventHandlers, 
            IShapeFactory shapeFactory) {

            _auditTrailRepository = auditTrailRepository;
            _providers = providers;
            _clock = clock;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            New = shapeFactory;
        }

        public dynamic New { get; set; }

        public IPageOfItems<AuditTrailEventRecord> GetPage(int page, int pageSize) {
            var totalItemCount = _auditTrailRepository.Table.Count();
            var startIndex = (page - 1)*pageSize;
            var pageOfData = _auditTrailRepository.Table.Skip(startIndex).Take(pageSize);

            return new PageOfItems<AuditTrailEventRecord>(pageOfData) {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalItemCount
            };
        }

        public AuditTrailEventRecord GetRecord(int id) {
            return _auditTrailRepository.Get(id);
        }

        public AuditTrailEventRecord Record(string eventName, IUser user, IContent content = null, IDictionary<string, object> eventData = null) {
            if(eventData == null)
                eventData = new Dictionary<string, object>();

            var createContext = new AuditTrailCreateContext {
                Event = eventName,
                User = user,
                Content = content,
                EventData = eventData
            };

            _auditTrailEventHandlers.Create(createContext);

            var eventDescriptor = GetEventDescriptor(eventName);

            var record = new AuditTrailEventRecord {
                Category = eventDescriptor.Category,
                Event = eventDescriptor.Event,
                ContentItemVersion = content != null ? content.ContentItem.VersionRecord : null,
                CreatedUtc = _clock.UtcNow,
                UserName = user != null ? user.UserName : null,
                EventData = SerializeEventData(eventData)
            };

            _auditTrailRepository.Create(record);
            return record;
        }

        public IEnumerable<AuditTrailCategoryDescriptor> Describe() {
            var context = new DescribeContext();
            foreach (var provider in _providers) {
                provider.Describe(context);                
            }
            return context.Describe();
        }

        public dynamic BuildDisplay(AuditTrailEventRecord record, string displayType) {
            var eventData = DeserializeEventData(record.EventData);
            var auditTrailEventShape = New.AuditTrailEvent(Record: record, EventData: eventData);
            var metaData = (ShapeMetadata)auditTrailEventShape.Metadata;
            metaData.DisplayType = displayType;
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}", displayType));
            metaData.Alternates.Add(String.Format("AuditTrailEvent__{0}", record.Category));
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}__{1}", displayType, record.Category));
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}__{1}", record.Category, record.Event));
            metaData.Alternates.Add(String.Format("AuditTrailEvent_{0}__{1}__{2}", displayType, record.Category, record.Event));
            return auditTrailEventShape;
        }

        private AuditTrailEventDescriptor GetEventDescriptor(string eventName) {
            var categoryDescriptors = Describe();
            var eventDescriptorQuery = from c in categoryDescriptors
                                       from e in c.Events
                                       where e.Event == eventName
                                       select e;
            var eventDescriptors = eventDescriptorQuery.ToArray();

            if (!eventDescriptors.Any()) {
                throw new ArgumentException(String.Format("No event named '{0}' exists.", eventName));
            }

            // TODO: Do we actually want this requirement, or should we support multiple events with the same name across categories?
            // SOLVED: We will use the fully qualified provider type name + event name.
            if (eventDescriptors.Count() > 1)
                throw new InvalidProgramException(String.Format("The specified event name occurs {0} times. Event names must be unique across categories.", eventDescriptors.Count()));

            return eventDescriptors.First();
        }

        private string SerializeEventData(IDictionary<string, object> eventData) {
            try {
                var json = JsonConvert.SerializeObject(eventData, Formatting.None);
                var xml = JsonConvert.DeserializeXNode(json, deserializeRootElementName: "EventData");
                return xml.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error during serialization of eventData");
            }
            return null;
        }

        private IDictionary<string, object> DeserializeEventData(string eventData) {
            if(String.IsNullOrWhiteSpace(eventData))
                return new Dictionary<string, object>();

            try {
                var node = XDocument.Parse(eventData);
                var json = JsonConvert.SerializeXNode(node, Formatting.None, omitRootObject: true);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error during deserialization of eventData");
            }
            return new Dictionary<string, object>();
        }
    }
}