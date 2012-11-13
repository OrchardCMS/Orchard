using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;

namespace Orchard.Workflows.Models {
    public class WorkflowDefinitionRecord {
        public WorkflowDefinitionRecord() {
            Activities = new List<ActivityRecord>();
            Connections = new List<ConnectionRecord>();
        }

        public virtual int Id { get; set; }
        public virtual bool Enabled { get; set; }

        [Required, StringLength(1024)]
        public virtual string Name { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<ActivityRecord> Activities { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<ConnectionRecord> Connections { get; set; }
    }
}