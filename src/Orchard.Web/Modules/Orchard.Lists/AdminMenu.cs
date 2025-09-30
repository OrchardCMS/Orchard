using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Lists {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) =>
            builder
                .AddImageSet("lists")
                .Add(T("Lists"), "11", item => item
                .Action("Index", "Admin", new { area = "Orchard.Lists" }).Permission(Permissions.ManageLists));
    }
}
