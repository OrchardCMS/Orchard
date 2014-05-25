using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.Collections;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;

namespace Orchard.AuditTrail.Services {
    public class AuditTrailManager : Component, IAuditTrailManager {
        private readonly IRepository<AuditTrailEventRecord> _auditTrailRepository;
        private readonly IAuditTrailEventProvider _providers;
        private readonly IClock _clock;
        private readonly IAuditTrailEventHandler _auditTrailEventHandlers;
        private readonly IEventDataSerializer _serializer;

        public AuditTrailManager(
            IRepository<AuditTrailEventRecord> auditTrailRepository,
            IAuditTrailEventProvider providers, 
            IClock clock,
            IAuditTrailEventHandler auditTrailEventHandlers, 
            IEventDataSerializer serializer) {

            _auditTrailRepository = auditTrailRepository;
            _providers = providers;
            _clock = clock;
            _auditTrailEventHandlers = auditTrailEventHandlers;
            _serializer = serializer;
        }

        public IPageOfItems<AuditTrailEventRecord> GetRecords(int page, int pageSize, AuditTrailFilterParameters filter = null, AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending) {
            
            var query = _auditTrailRepository.Table;

            if (filter != null) {
                if (!String.IsNullOrWhiteSpace(filter.FilterKey)) query = query.Where(x => x.EventFilterKey == filter.FilterKey);
                if (!String.IsNullOrWhiteSpace(filter.FilterValue)) query = query.Where(x => x.EventFilterData == filter.FilterValue);
                if (!String.IsNullOrWhiteSpace(filter.UserName)) query = query.Where(x => x.UserName == filter.UserName);
                if (filter.From != null) query = query.Where(x => x.CreatedUtc >= filter.From);
                if (filter.To != null) query = query.Where(x => x.CreatedUtc <= filter.To);
            }

            switch (orderBy) {
                default:
                    query = query.OrderByDescending(x => x.CreatedUtc).ThenByDescending(x => x.Id);
                    break;
                case AuditTrailOrderBy.EventAscending:
                    query = query.OrderBy(x => x.Event).ThenByDescending(x => x.Id);
                    break;
            }

            var totalItemCount = query.Count();
            var startIndex = (page - 1) * pageSize;
            var records = query.Skip(startIndex).Take(pageSize);

            return new PageOfItems<AuditTrailEventRecord>(records) {
                PageNumber = page,
                PageSize = pageSize,
                TotalItemCount = totalItemCount
            };
        }

        public AuditTrailEventRecord GetRecord(int id) {
            return _auditTrailRepository.Get(id);
        }

        public AuditTrailEventRecord Record<T>(string eventName, IUser user, IDictionary<string, object> properties = null, IDictionary<string, object> eventData = null, string eventFilterKey = null, string eventFilterData = null) where T:IAuditTrailEventProvider {
            if (properties == null) properties = new Dictionary<string, object>();
            if (eventData == null) eventData = new Dictionary<string, object>();

            var context = new AuditTrailCreateContext {
                Event = eventName,
                User = user,
                Properties = properties,
                EventData = eventData,
                EventFilterKey = eventFilterKey,
                EventFilterData = eventFilterData
            };

            _auditTrailEventHandlers.Create(context);
            var eventDescriptor = Describe<T>(eventName);

            var record = new AuditTrailEventRecord {
                Category = eventDescriptor.CategoryDescriptor.Category,
                Event = eventDescriptor.Event,
                CreatedUtc = _clock.UtcNow,
                UserName = user != null ? user.UserName : null,
                EventData = _serializer.Serialize(context.EventData),
                EventFilterKey = context.EventFilterKey,
                EventFilterData = context.EventFilterData,
                Comment = context.Comment
            };

            _auditTrailRepository.Create(record);
            return record;
        }

        public IEnumerable<AuditTrailCategoryDescriptor> Describe() {
            var context = new DescribeContext();
            _providers.Describe(context);
            return context.Describe();
        }

        public AuditTrailEventDescriptor Describe<T>(string eventName) where T:IAuditTrailEventProvider {
            var fullyQualifiedEventName = EventNameHelper.GetFullyQualifiedEventName<T>(eventName);
            return Describe(fullyQualifiedEventName);
        }

        public AuditTrailEventDescriptor Describe(string fullyQualifiedEventName) {
            var categoryDescriptors = Describe();
            var eventDescriptorQuery = from c in categoryDescriptors
                                       from e in c.Events
                                       where e.Event == fullyQualifiedEventName
                                       select e;
            var eventDescriptors = eventDescriptorQuery.ToArray();

            if (!eventDescriptors.Any()) {
                throw new ArgumentException(String.Format("No event named '{0}' exists.", fullyQualifiedEventName));
            }

            return eventDescriptors.First();
        }
    }
}