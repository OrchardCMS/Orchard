using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Workflows.ViewModels {
    public class WorkflowDefinitionViewModel {
        public WorkflowDefinitionViewModel() {
            Activities = new List<ActivityViewModel>();
            Connections = new List<ConnectionViewModel>();
        }
        public int Id { get; set; }

        /// <summary>
        ///  Used to prevent client side LocalStorage conflicts
        /// </summary>
        public string Tenant { get; set; }

        public List<ActivityViewModel> Activities { get; set; }
        public List<ConnectionViewModel> Connections { get; set; }
    }

    public class ActivityViewModel {
        /// <summary>
        /// The local id used for connections
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The name of the activity
        /// </summary>
        public string Name { get; set; }

        public IDictionary<string, string> State { get; set; }
    }

    public class ConnectionViewModel {
        public int Id { get; set; }
        public string SourceClientId { get; set; }
        public string Outcome { get; set; }

        public string DestinationClientId { get; set; }
    }

}