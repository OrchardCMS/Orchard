using System;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.ViewModels {
    public class DeploymentHistoryViewModel {
        public string Source { get; set; }
        public string Target { get; set; }
        public string ExecutionId { get; set; }
        public string RecipeStatus { get; set; }
        public DeploymentType DeploymentType { get; set; }
        public string SubscriptionName { get; set; }
        public int? SubscriptionId { get; set; }
        public DateTime? RunStarted { get; set; }
        public DateTime? RunCompleted { get; set; }
        public RunStatus RunStatus { get; set; }
        public bool RecipeFileAvailable { get; set; }
    }
}