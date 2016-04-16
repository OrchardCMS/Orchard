using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.ViewModels {
    public class SubscriptionPartViewModel {
        public ContentItem ContentItem { get; set; }
        public string DeploymentType { get; set; }
        public int DeploymentConfigurationId { get; set; }
        public string DeploymentDescription { get; set; }
        public bool Metadata { get; set; }
        public bool Data { get; set; }
        public bool Files { get; set; }
        public bool DeployAsDraft { get; set; }
        public bool UsePredefinedQuery { get; set; }
        public string DataImportChoice { get; set; }
        public string FilterChoice { get; set; }
        public IList<string> SelectedContentTypes { get; set; }
        public string SelectedQueryIdentity { get; set; }
        public IList<DeploymentContentType> ContentTypes { get; set; }
        public IList<CustomStepEntry> CustomSteps { get; set; }
        public IList<DeploymentQuery> Queries { get; set; }
        public IList<IContent> DeploymentConfigurations { get; set; }

        public string DeployedChangesToDisplay { get; set; }
        public string DeployedChangesToDate { get; set; }
        public string DeployedChangesToTime { get; set; }
    }
}