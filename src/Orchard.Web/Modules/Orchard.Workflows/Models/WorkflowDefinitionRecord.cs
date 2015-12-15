using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;

namespace Orchard.Workflows.Models {
    /// <summary>
    /// Represent a workflow definition comprised of activities and transitions between them.
    /// </summary>
    public class WorkflowDefinitionRecord {
        public WorkflowDefinitionRecord() {
            ActivityRecords = new List<ActivityRecord>();
            TransitionRecords = new List<TransitionRecord>();
        }

        public virtual int Id { get; set; }

        /// <summary>
        /// Whether or not to enable workflows of this type.
        /// </summary>
        public virtual bool Enabled { get; set; }

        /// <summary>
        /// The name of the workflow definition.
        /// </summary>
        [Required, StringLength(1024)]
        public virtual string Name { get; set; }

        /// <summary>
        /// List of <see cref="ActivityRecord"/> composing this workflow definition.
        /// </summary>
        [CascadeAllDeleteOrphan, Aggregate]
        public virtual IList<ActivityRecord> ActivityRecords { get; set; }

        /// <summary>
        /// List of <see cref="TransitionRecord"/> composing this workflow definition.
        /// This is distinct from Activities as there could be activities without 
        /// any connection an any time of the design process, though they should
        /// be synchronized.
        /// </summary>
        [CascadeAllDeleteOrphan, Aggregate]
        public virtual IList<TransitionRecord> TransitionRecords { get; set; }

        /// <summary>
        /// List of <see cref="WorkflowRecord"/> associated with this workflow definition.
        /// </summary>
        [CascadeAllDeleteOrphan]
        public virtual IList<WorkflowRecord> WorkflowRecords { get; set; }
    }
}