using Orchard.ContentManagement.Handlers;
using Orchard.Dashboards.Models;

namespace Orchard.Dashboards.Handlers {
    public class DashboardSiteSettingsPartHandler : ContentHandler {
        public DashboardSiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<DashboardSiteSettingsPart>("Site"));
        }
    }
}