namespace Orchard.Workflows.Models {
    public class AwaitingActivityRecord {
        public virtual int Id { get; set; }

        public virtual ActivityRecord ActivityRecord { get; set; }

        // Parent property
        public virtual WorkflowRecord WorkflowRecord { get; set; }
    }
}