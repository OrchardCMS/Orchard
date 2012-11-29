using System.Collections.Generic;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Workflows.ViewModels {
    public class AdminEditViewModel {
        public WorkflowDefinitionRecord WorkflowDefinitionRecord { get; set; }
        public IEnumerable<IActivity> AllActivities { get; set; }
    }
}