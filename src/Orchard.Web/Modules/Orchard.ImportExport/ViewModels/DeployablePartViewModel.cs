using System;
using System.Collections.Generic;
using Orchard.Core.Common.Models;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.ViewModels
{
    public class DeployablePartViewModel {
        public CommonPart Part { get; set; }
        public bool HasDraftVersion { get; set; }
        public bool HasPublishedVersion { get; set; }
        public List<DeployablePartTargetSummary> Targets { get; set; } 
    }

    public class DeployablePartTargetSummary {
        public string Target { get; set; }
        public int TargetId { get; set; }
        public DateTime? LastDeploy { get; set; }
        public DeploymentStatus Status { get; set; }
    }
}