using System.Collections.Generic;

namespace Orchard.Rules.Models {
    public class EventContext {
        public EventContext() {
            Tokens = new Dictionary<string, object>();
            Properties = new Dictionary<string, string>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public IDictionary<string, string> Properties { get; set; }
    }
}