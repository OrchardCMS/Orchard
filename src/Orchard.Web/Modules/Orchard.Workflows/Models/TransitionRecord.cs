namespace Orchard.Workflows.Models {
    /// <summary>
    /// Reprensents a transition between two <see cref="ActivityRecord"/>
    /// </summary>
    public class TransitionRecord {

        public virtual int Id { get; set; }

        /// <summary>
        /// The source <see cref="ActivityRecord"/>
        /// </summary>
        public virtual ActivityRecord SourceActivityRecord { get; set; }

        /// <summary>
        /// The name of the endpoint on the source <see cref="ActivityRecord"/>
        /// </summary>
        public virtual string SourceEndpoint { get; set; }

        /// <summary>
        /// The destination <see cref="ActivityRecord"/>
        /// </summary>
        public virtual ActivityRecord DestinationActivityRecord { get; set; }

        /// <summary>
        /// The name of the endpoint on the destination <see cref="ActivityRecord"/>
        /// </summary>
        public virtual string DestinationEndpoint { get; set; }

        /// <summary>
        /// The parent <see cref="WorkflowDefinitionRecord"/>
        /// </summary>
        public virtual WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }
    }
}