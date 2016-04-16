using System;

namespace Orchard.ImportExport.Models {
    public class ItemDeploymentEntry {
        public int TargetId { get; set; }
        public DateTime? DeploymentCompletedUtc { get; set; }
        public DeploymentStatus Status { get; set; }
        public string Description { get; set; }
    }
}