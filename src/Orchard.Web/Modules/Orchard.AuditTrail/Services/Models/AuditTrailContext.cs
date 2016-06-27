using System.Collections.Generic;
using Orchard.Security;

namespace Orchard.AuditTrail.Services.Models {
    public class AuditTrailContext {
        public AuditTrailContext() {
            EventData = new Dictionary<string, object>();
        }

        public string Event { get; set; }
        public IUser User { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public IDictionary<string, object> EventData { get; set; }
        public string EventFilterKey { get; set; }
        public string EventFilterData { get; set; }
    }
}