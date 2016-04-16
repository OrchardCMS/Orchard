using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.ImportExport.ViewModels {
    public class CreateSubscriptionViewModel {
        public string Title { get; set; }
        public string SelectedDeploymentType { get; set; }
        public int SelectedDeploymentSourceId { get; set; }
        public int SelectedDeploymentTargetId { get; set; }
        public List<string> SubscriptionTypes { get; set; }
        public List<IContent> Sources { get; set; }
        public List<IContent> Targets { get; set; }
    }
}