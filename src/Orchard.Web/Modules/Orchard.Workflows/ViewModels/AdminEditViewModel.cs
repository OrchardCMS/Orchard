using System.Collections.Generic;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.ViewModels {
    public class AdminEditViewModel {
        public string LocalId { get; set; }
        public IEnumerable<IActivity> AllActivities { get; set; }
        public WorkflowDefinitionViewModel WorkflowDefinition { get; set; }
    }
}