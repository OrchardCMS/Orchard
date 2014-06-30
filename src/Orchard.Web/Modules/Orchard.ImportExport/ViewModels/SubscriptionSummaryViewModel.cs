using System;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.ViewModels {
    public class SubscriptionSummaryViewModel {
        public int Id { get; set; }
        public string Name { get; set; }
        public DeploymentType DeploymentType { get; set; }
        public string Source { get; set; }
        public DateTime? LastRunDateTime { get; set; }
        public string LastRunStatus { get; set; }
        public DateTime? NextRun { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}
