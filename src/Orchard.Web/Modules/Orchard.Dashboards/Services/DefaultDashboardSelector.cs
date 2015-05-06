using Orchard.ContentManagement;
using Orchard.Dashboards.Models;

namespace Orchard.Dashboards.Services {
    public class DefaultDashboardSelector : IDashboardSelector {
        private readonly IOrchardServices _services;
        public DefaultDashboardSelector(IOrchardServices services) {
            _services = services;
        }

        public DashboardDescriptor GetDashboardDescriptor() {
            var settings = _services.WorkContext.CurrentSite.As<DashboardSiteSettingsPart>();
            var dashboardId = settings.DefaultDashboardId;
            var dashboard = dashboardId != null ? _services.ContentManager.Get(dashboardId.Value) : default(ContentItem);
            var descriptor = new DashboardDescriptor { Priority = -10 };

            if (dashboard == null)
                descriptor.DashboardFactory = shapeFactory => shapeFactory.StaticDashboard();
            else
                descriptor.DashboardFactory = shapeFactory => _services.ContentManager.BuildDisplay(dashboard, displayType: "Dashboard");

            return descriptor;
        }
    }
}