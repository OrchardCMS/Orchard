using Orchard.ContentManagement;

namespace Orchard.Dashboards.ViewModels {
    public class DashboardSiteSettingsViewModel {
        public string SelectedDashboardId { get; set; }
        public ContentItem SelectedDashboard { get; set; }
    }
}