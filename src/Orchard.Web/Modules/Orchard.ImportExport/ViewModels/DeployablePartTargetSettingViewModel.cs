using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class DeployableTypePartSettingsViewModel {
        public List<Target> Targets { get; set; }
        public List<string> AvailableTriggers { get; set; }
        public List<string> AvailableActions { get; set; }

        public class Target {
            public int TargetId { get; set; }
            public string TargetName { get; set; }
            public string SelectedTrigger { get; set; }
            public string SelectedAction { get; set; }
        }
    }
}