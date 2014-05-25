using System.Collections.Generic;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Services;
using Orchard.Localization;

namespace Orchard.AuditTrail.Models {
    public class DescribeFor {
        private readonly IList<AuditTrailEventDescriptor> _events = new List<AuditTrailEventDescriptor>();

        public DescribeFor(string category, LocalizedString name) {
            Category = category;
            Name = name;
        }

        public IEnumerable<AuditTrailEventDescriptor> Events {
            get { return _events; }
        }

        public string Category { get; private set; }
        public LocalizedString Name { get; private set; }

        public DescribeFor Event(IAuditTrailEventProvider provider, string eventName, LocalizedString name, LocalizedString description, bool enableByDefault = false) {
            _events.Add(new AuditTrailEventDescriptor {
                CategoryDescriptor = new AuditTrailCategoryDescriptor {
                    Category = Category,
                    Name = Name,
                    Events = Events
                }, 
                Event = EventNameHelper.GetFullyQualifiedEventName(provider.GetType(), eventName), 
                Name = name, 
                Description = description, 
                IsEnabledByDefault = enableByDefault
            });
            return this;
        }
    }
}