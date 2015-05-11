using Orchard.ContentManagement;

namespace Orchard.Dashboards.Services {
    public class DashboardSelectorResult {
        public int Priority { get; set; }
        public IContent Dashboard { get; set; }
    }
}