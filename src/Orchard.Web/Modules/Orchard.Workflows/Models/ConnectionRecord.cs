namespace Orchard.Workflows.Models {
    public class ConnectionRecord {

        public virtual int Id { get; set; }

        // Source is null if it's a starting activity
        public virtual ActivityRecord Source { get; set; }
        public virtual string SourceEndpoint { get; set; }

        // Destination is null if it's an ending activity
        public virtual ActivityRecord Destination { get; set; }
        public virtual string DestinationEndpoint { get; set; }

        // Parent relationship
        public virtual WorkflowDefinitionRecord WorkflowDefinition { get; set; }
    }
}