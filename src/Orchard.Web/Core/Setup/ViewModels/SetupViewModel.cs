using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Setup.ViewModels {
    public class SetupViewModel : BaseViewModel {
        public string SiteName { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public string DatabaseConnectionString { get; set; }
    }
}