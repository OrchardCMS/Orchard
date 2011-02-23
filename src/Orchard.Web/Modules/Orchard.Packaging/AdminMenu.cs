using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.UI.Navigation;
using Orchard.Security;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        private readonly IBackgroundPackageUpdateStatus _backgroundPackageUpdateStatus;

        public AdminMenu(IBackgroundPackageUpdateStatus backgroundPackageUpdateStatus) {
            _backgroundPackageUpdateStatus = backgroundPackageUpdateStatus;
        }

        public AdminMenu() {}

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Themes"), menu => menu
                .Add(T("Gallery"), "1", item => item.Action("Themes", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Modules"), menu => menu
                .Add(T("Gallery"), "2", item => item.Action("Modules", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Settings"), menu => menu
                .Add(T("Gallery"), "1", item => item.Action("Sources", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner)));

            builder.Add(T("Modules"), menu => menu
                .Add(T("Install"), "99", item => item.Action("AddModule", "PackagingServices", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Themes"), menu => menu
                .Add(T("Install"), "99", item => item.Action("AddTheme", "PackagingServices", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            if (_backgroundPackageUpdateStatus != null) {
                // Only available if feature is enabled

                int modulesUpdateCount = GetUpdateCount(DefaultExtensionTypes.Module);
                LocalizedString modulesCaption = (modulesUpdateCount == 0 ? T("Updates") : T("Updates ({0})", modulesUpdateCount));

                int themesUpdateCount = GetUpdateCount(DefaultExtensionTypes.Theme);
                LocalizedString themesCaption = (themesUpdateCount == 0 ? T("Updates") : T("Updates ({0})", themesUpdateCount));

                builder.Add(T("Modules"), "20", menu => menu
                    .Add(modulesCaption, "30.0", item => item.Action("ModulesUpdates", "GalleryUpdates", new { area = "Orchard.Packaging" })
                        .Permission(StandardPermissions.SiteOwner).LocalNav()));

                builder.Add(T("Themes"), "25", menu => menu
                    .Add(themesCaption, "30.0", item => item.Action("ThemesUpdates", "GalleryUpdates", new { area = "Orchard.Packaging" })
                        .Permission(StandardPermissions.SiteOwner).LocalNav()));
            }
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