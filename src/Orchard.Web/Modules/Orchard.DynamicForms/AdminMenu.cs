using Orchard.UI.Navigation;

namespace Orchard.DynamicForms {
    public class AdminMenu : Component, INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("dynamicforms")
                .Add(T("Dynamic Forms"), "4", menu => menu
                    .Add(T("Manage Forms"), "1.0",
                        item => item
                            .Action("Index", "Admin", new { area = "Orchard.DynamicForms" })
                            .Permission(Permissions.ManageForms))
            );
        }
    }
}
