using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Security;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailContext {
        public string Event { get; set; }
        public IUser User { get; set; }
        public IContent Content { get; set; }
        public IDictionary<string, object> EventData { get; set; }
    }
}