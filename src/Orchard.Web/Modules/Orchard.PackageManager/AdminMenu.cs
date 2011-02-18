using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.PackageManager.Services;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.PackageManager {
    public class AdminMenu : INavigationProvider {
        private readonly IBackgroundPackageUpdateStatus _backgroundPackageUpdateStatus;

        public AdminMenu(IBackgroundPackageUpdateStatus backgroundPackageUpdateStatus) {
            _backgroundPackageUpdateStatus = backgroundPackageUpdateStatus;
        }

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            int modulesUpdateCount = GetUpdateCount(DefaultExtensionTypes.Module);
            LocalizedString modulesCaption = (modulesUpdateCount == 0 ? T("Updates") : T("Updates ({0})", modulesUpdateCount));

            int themesUpdateCount = GetUpdateCount(DefaultExtensionTypes.Module);
            LocalizedString themesCaption = (themesUpdateCount == 0 ? T("Updates") : T("Updates ({0})", themesUpdateCount));

            builder.Add(T("Modules"), "20", menu => menu
                .Add(modulesCaption, "30.0", item => item.Action("ModulesUpdates", "Admin", new { area = "Orchard.PackageManager" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Themes"), "25", menu => menu
                .Add(themesCaption, "30.0", item => item.Action("ThemesUpdates", "Admin", new { area = "Orchard.PackageManager" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));
        }

        private int GetUpdateCount(string extensionType) {
            try {
                // Admin menu should never block, so simply return the result from the background task
                return _backgroundPackageUpdateStatus.Value == null ?
                    0 :
                    _backgroundPackageUpdateStatus.Value.Entries.Where(updatePackageEntry =>
                        updatePackageEntry.NewVersionToInstall != null &&
                        updatePackageEntry.ExtensionsDescriptor.ExtensionType.Equals(extensionType)).Count();
            } catch {
                return 0;
            }
        }
    }
}