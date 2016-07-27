using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Layouts {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("layouts")
                .Add(T("Layouts"), "9",
                    layouts => layouts.Action("List", "Admin", new {id = "Layout", area = "Contents"}).Permission(Permissions.ManageLayouts)
                        .Add(T("Layouts"), "0", elements => elements.Action("List", "Admin", new { id = "Layout", area = "Contents" }).Permission(Permissions.ManageLayouts))
                        .Add(T("Elements"), "1", elements => elements.Action("Index", "BlueprintAdmin", new {area = "Orchard.Layouts"}).Permission(Permissions.ManageLayouts)),
                    new[] {"browser"});
        }
    }
}