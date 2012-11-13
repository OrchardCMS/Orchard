using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Workflows.Models {
    public class WorkflowRecord {
        public WorkflowRecord() {
            AwaitingActivities = new List<ActivityRecord>();
        }

        public virtual int Id { get; set; }

        [StringLengthMax]
        public virtual string State { get; set; }

        public virtual IList<ActivityRecord> AwaitingActivities { get; set; }
    }
}