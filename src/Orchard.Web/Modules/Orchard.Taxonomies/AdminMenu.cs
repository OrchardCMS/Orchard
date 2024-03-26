using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Taxonomies {
    public class AdminMenu : INavigationProvider {
        private readonly IOrchardServices _services;
        public AdminMenu(IOrchardServices services) {
            _services = services;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("taxonomies")
                .Add(T("Taxonomies"), "4", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {

            if (_services.Authorizer.Authorize(Permissions.MergeTerms) || _services.Authorizer.Authorize(Permissions.EditTerm) || _services.Authorizer.Authorize(Permissions.CreateTerm) || _services.Authorizer.Authorize(Permissions.DeleteTerm)) {
                menu.Add(T("Manage Taxonomies"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Taxonomies" }));
            }
        }
    }

}
