using System;
using Orchard.ContentManagement;
using Orchard.Dashboards.Models;

namespace Orchard.Dashboards.Services {
    public class DefaultDashboardSelector : IDashboardSelector {
        private readonly IOrchardServices _services;
        public DefaultDashboardSelector(IOrchardServices services) {
            _services = services;
        }

        public DashboardSelectorResult GetDashboard() {
            var settings = _services.WorkContext.CurrentSite.As<DashboardSiteSettingsPart>();
            var dashboardId = settings.DefaultDashboardId;
            var dashboard = dashboardId != null ? _services.ContentManager.Get(dashboardId.Value) : default(ContentItem);
        }
    }
}