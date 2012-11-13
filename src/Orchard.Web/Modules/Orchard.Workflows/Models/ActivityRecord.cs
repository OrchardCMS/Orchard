using System.Collections.Generic;

namespace Orchard.Workflows.Models {
    public class ActivityRecord {
        public virtual int Id { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string Parameters { get; set; }

        // Parent property
        public virtual WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }

        // Parent property
        public virtual IList<ActivityRecord> RegisteredWorkflows { get; set; }
    }
}