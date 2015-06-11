using System.Collections.Generic;
using Orchard.AuditTrail.Helpers;
using Orchard.Localization;

namespace Orchard.AuditTrail.Services.Models {
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

        public DescribeFor Event(
            IAuditTrailEventProvider provider, 
            string eventName, 
            LocalizedString name, 
            LocalizedString description, 
            bool enableByDefault = false, 
            bool isMandatory = false) {

            _events.Add(new AuditTrailEventDescriptor {
                CategoryDescriptor = new AuditTrailCategoryDescriptor {
                    Category = Category,
                    Name = Name,
                    Events = Events
                }, 
                Event = EventNameExtensions.GetFullyQualifiedEventName(provider.GetType(), eventName), 
                Name = name, 
                Description = description, 
                IsEnabledByDefault = enableByDefault,
                IsMandatory = isMandatory
            });

            return this;
        }
    }
}