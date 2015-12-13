using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.CustomForms {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("customforms")
                .Add(T("Custom Forms"), "4",
                menu => menu
                    .Add(T("Manage Forms"), "1.0",
                        item => item.Action("Index", "Admin", new { area = "Orchard.CustomForms" }).Permission(Permissions.ManageForms))
            );
        }
    }
}
