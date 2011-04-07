using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Comments {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("comments")
                .Add(T("Comments"), "4",
                    menu => menu.Add(T("List"), "0", item => item.Action("Index", "Admin", new { area = "Orchard.Comments" })
                        .Permission(Permissions.ManageComments)));
        }
    }
}
