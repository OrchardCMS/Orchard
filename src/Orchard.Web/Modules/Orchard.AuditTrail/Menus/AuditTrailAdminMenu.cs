using Orchard.UI.Navigation;

namespace Orchard.AuditTrail.Menus {
    public class AuditTrailAdminMenu : Component, INavigationProvider {
        
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("audittrail")
                .Add(T("Audit Trail"), "12", auditTrail => auditTrail
                    .Action("Index", "Admin", new { area = "Orchard.AuditTrail" })
                    .Permission(Permissions.ManageAuditTrailSettings)
                    .Add(T("History"), "1", history => history
                        .Action("Index", "Admin", new { area = "Orchard.AuditTrail" })
                        .LocalNav()));
        }
    }
}