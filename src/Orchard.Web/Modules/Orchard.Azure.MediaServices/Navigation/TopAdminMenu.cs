using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Azure.MediaServices.Navigation {
    public class TopAdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "top_admin"; } }

        public void GetNavigation(NavigationBuilder builder) {

            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Microsoft Azure Media"), "10.0", item => item.Action("Index", "Settings", new { area = "Orchard.Azure.MediaServices" })
                        .Permission(StandardPermissions.SiteOwner)
                        .Permission(Permissions.ManageCloudMediaSettings)
                ));
        }
    }
}
