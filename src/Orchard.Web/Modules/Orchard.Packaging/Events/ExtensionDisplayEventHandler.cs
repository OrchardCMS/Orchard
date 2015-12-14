using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;

namespace Orchard.Packaging.Events {
    [OrchardFeature("Gallery.Updates")]
    public class ExtensionDisplayEventHandler : IExtensionDisplayEventHandler {
        private readonly IBackgroundPackageUpdateStatus _backgroundPackageUpdateStatus;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IPackageUpdateService _packageUpdateService;

        public ExtensionDisplayEventHandler(IBackgroundPackageUpdateStatus backgroundPackageUpdateStatus,
            IPackagingSourceManager packagingSourceManager,
            IPackageUpdateService packageUpdateService) {

            _backgroundPackageUpdateStatus = backgroundPackageUpdateStatus;
            _packagingSourceManager = packagingSourceManager;
            _packageUpdateService = packageUpdateService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<string> Displaying(ExtensionDescriptor extensionDescriptor, RequestContext requestContext) {
            // Get status from background task state or directly
            _backgroundPackageUpdateStatus.Value =
                _backgroundPackageUpdateStatus.Value ??
                _packageUpdateService.GetPackagesStatus(_packagingSourceManager.GetSources());

            UpdatePackageEntry updatePackageEntry = _backgroundPackageUpdateStatus.Value.Entries
                .Where(package => package.ExtensionsDescriptor.Id.Equals(extensionDescriptor.Id)).FirstOrDefault();

            if (updatePackageEntry != null) {
                if (updatePackageEntry.NewVersionToInstall != null) {
                    UrlHelper urlHelper = new UrlHelper(requestContext);
                    yield return T("Version {0} is available (See <a href=\"{1}\">Updates</a>).", updatePackageEntry.NewVersionToInstall.Version,
                                    urlHelper.Action(DefaultExtensionTypes.IsTheme(updatePackageEntry.ExtensionsDescriptor.ExtensionType)
                                            ? "ThemesUpdates"
                                            : "ModulesUpdates",
                                            "GalleryUpdates",
                                            new { Area = "Orchard.Packaging" })).ToString();
                }
            }
        }
    }
}