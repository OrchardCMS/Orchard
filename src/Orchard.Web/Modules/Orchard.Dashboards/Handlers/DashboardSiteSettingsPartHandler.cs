using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Dashboards.Models;
using Orchard.Localization;

namespace Orchard.Dashboards.Handlers {
    public class DashboardSiteSettingsPartHandler : ContentHandler {
        public DashboardSiteSettingsPartHandler() {
            Filters.Add(new ActivatingFilter<DashboardSiteSettingsPart>("Site"));
            OnGetContentItemMetadata<DashboardSiteSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Dashboard"))));

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
    }
}