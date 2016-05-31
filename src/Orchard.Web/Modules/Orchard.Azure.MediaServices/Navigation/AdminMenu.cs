using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Azure.MediaServices.Navigation {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Media"), "6",
                    menu => menu
                        .Add(T("Microsoft Azure Media Jobs"), "10.0", item => item.Action("Index", "Job", new {area = "Orchard.Azure.MediaServices"})
                            .Permission(Permissions.ManageCloudMediaJobs)),
                    new[] {"photo"});
        }
    }
}
