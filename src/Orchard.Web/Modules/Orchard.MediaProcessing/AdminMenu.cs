using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.MediaProcessing {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Media"), "6", item => item.Add(T("Profiles"), "5", i => i.Action("Index", "Admin", new {area = "Orchard.MediaProcessing"}).Permission(StandardPermissions.SiteOwner)));
        }
    }
}
