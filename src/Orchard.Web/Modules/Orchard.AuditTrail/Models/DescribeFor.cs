using System.Collections.Generic;
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

        public DescribeFor Event(string eventName, LocalizedString name, LocalizedString description, bool enableByDefault = false) {
            _events.Add(new AuditTrailEventDescriptor {
                Category = Category, 
                Event = eventName, 
                Name = name, 
                Description = description, 
                IsEnabledByDefault = enableByDefault
            });
            return this;
        }
    }
}