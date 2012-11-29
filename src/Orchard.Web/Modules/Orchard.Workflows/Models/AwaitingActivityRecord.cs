using System.Collections.Generic;

namespace Orchard.Workflows.Models {
    public class AwaitingActivityRecord {
        public virtual int Id { get; set; }

        public virtual ActivityRecord ActivityRecord { get; set; }
        
        // Parent property
        public virtual IList<WorkflowRecord> WorkflowRecords { get; set; }
    }
}