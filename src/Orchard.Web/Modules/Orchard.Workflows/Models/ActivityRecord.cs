using Orchard.Data.Conventions;

namespace Orchard.Workflows.Models {
    /// <summary>
    /// Represents an activity in a <see cref="WorkflowDefinitionRecord"/>
    /// </summary>
    public class ActivityRecord {
        public virtual int Id { get; set; }
        
        /// <summary>
        /// The type of the activity.
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// The serialized state of the activity.
        /// </summary>
        [StringLengthMax]
        public virtual string State { get; set; }

        /// <summary>
        /// The parent <see cref="WorkflowDefinitionRecord"/> 
        /// containing this activity.
        /// </summary>
        public virtual WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }
    }
}