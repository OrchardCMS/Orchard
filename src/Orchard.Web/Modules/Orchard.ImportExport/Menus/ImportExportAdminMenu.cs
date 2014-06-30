using Orchard.ImportExport.Permissions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.ImportExport.Menus {
    public class ImportExportAdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("importexport")
                .Add(T("Import/Export"), "42", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Import"), "0", item => item
                .Action("Import", "Admin", new {area = "Orchard.ImportExport"})
                .Permission(ImportExportPermissions.Import)
                .LocalNav());
            menu.Add(T("Export"), "0", item => item
                .Action("Export", "Admin", new {area = "Orchard.ImportExport"})
                .Permission(ImportExportPermissions.Export)
                .LocalNav());
        }
    }
}
