using System.Collections.Generic;

namespace Orchard.Workflows.Models.Descriptors {
    public class ActivityContext {
        public ActivityContext() {
            Tokens = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
    }
}