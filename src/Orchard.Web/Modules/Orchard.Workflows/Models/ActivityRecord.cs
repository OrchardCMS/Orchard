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
        public virtual string Name { get; set; }

        /// <summary>
        /// The serialized state of the activity.
        /// </summary>
        [StringLengthMax]
        public virtual string State { get; set; }

        /// <summary>
        /// The left coordinate of the activity.
        /// </summary>
        public virtual int X { get; set; }

        /// <summary>
        /// The top coordinate of the activity.
        /// </summary>
        public virtual int Y { get; set; }

        /// <summary>
        /// Whether the activity is a start state for a WorkflowDefinition.
        /// </summary>
        public virtual bool Start { get; set; }

        /// <summary>
        /// The parent <see cref="WorkflowDefinitionRecord"/> 
        /// containing this activity.
        /// </summary>
        public virtual WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }


        /// <summary>
        /// Gets the Id which can be used on the client. 
        /// </summary>
        /// <returns>An unique Id to represent this activity on the client.</returns>
        public string GetClientId() {
            return Name + "_" + Id;
        }
    }
}