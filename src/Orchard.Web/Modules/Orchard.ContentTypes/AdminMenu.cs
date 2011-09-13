using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.ContentTypes {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("Content"),
                        menu => menu
                                    .Add(T("Content Types"), "3", item => item.Action("Index", "Admin", new {area = "Orchard.ContentTypes"}).LocalNav())
                                    .Add(T("Content Parts"), "4", item => item.Action("ListParts", "Admin", new {area = "Orchard.ContentTypes"}).LocalNav()));

        }
    }
}