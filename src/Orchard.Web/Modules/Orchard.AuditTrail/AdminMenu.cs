using Orchard.UI.Navigation;

namespace Orchard.AuditTrail {
    public class AdminMenu : Component, INavigationProvider {
        
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("audittrail")
                .Add(T("Audit Trail"), "12", menuItem => menuItem
                    .Action("Index", "Admin", new { area = "Orchard.AuditTrail" })
                    .Permission(Permissions.ManageAuditTrailSettings));
        }
    }
}