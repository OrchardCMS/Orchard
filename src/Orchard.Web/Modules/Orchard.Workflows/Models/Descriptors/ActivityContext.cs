using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Workflows.Models.Descriptors {
    public class ActivityContext {
        public ActivityContext() {
            Tokens = new Dictionary<string, object>();
        }

        /// <summary>
        /// If set, represents the subject of the current workflow
        /// </summary>
        public IContent Content { get; set; }

        public IDictionary<string, object> Tokens { get; set; }
        public dynamic State { get; set; }
        public dynamic WorkflowState { get; set; }
    }
}