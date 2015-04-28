using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.ViewModels
{
    public class DeployablePartViewModel {
        public CommonPart Part { get; set; }
        public bool IsDraftable { get; set; }
        public bool HasPublishedVersion { get; set; }
        public List<DeployablePartTargetSummary> Targets { get; set; } 
    }

    public class DeployablePartTargetSummary {
        public IContent Target { get; set; }
        public string TargetName { get; set; }
        public DateTime? LastDeploy { get; set; }
        public DeploymentStatus Status { get; set; }
        public string Description { get; set; }
    }
}