using Orchard.ContentManagement;

namespace Orchard.Dashboards.Models {
    public class DashboardSiteSettingsPart : ContentPart {
        public int? DefaultDashboardId {
            get { return this.Retrieve(x => x.DefaultDashboardId); }
            set { this.Store(x => x.DefaultDashboardId, value); }
        }
    }
}