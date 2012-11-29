using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Workflows.Models {
    /// <summary>
    /// Reprensents a running workflow instance.
    /// </summary>
    public class WorkflowRecord {
        public WorkflowRecord() {
            AwaitingActivities = new List<AwaitingActivityRecord>();
        }

        public virtual int Id { get; set; }

        /// <summary>
        /// Serialized state of the workflow.
        /// </summary>
        [StringLengthMax]
        public virtual string State { get; set; }

        /// <summary>
        /// List of activities the current workflow instance is waiting on 
        /// for continuing its process.
        /// </summary>
        [CascadeAllDeleteOrphan]
        public virtual IList<AwaitingActivityRecord> AwaitingActivities { get; set; }

        /// <summary>
        /// Parent <see cref="WorkflowDefinitionRecord"/>
        /// </summary>
        public virtual WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }

    }
}