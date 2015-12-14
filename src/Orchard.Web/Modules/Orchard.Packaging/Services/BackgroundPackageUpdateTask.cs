using Orchard.Environment.Extensions;
using Orchard.Tasks;

namespace Orchard.Packaging.Services {
    /// <summary>
    /// Background task responsible for fetching feeds from the Gallery into the
    /// BackgroundPackageUpdateStatus singleton dependency.
    /// The purpose is to make sure we don't block the Admin panel the first time
    /// it's accessed when the PackageManager feature is enabled. The first time
    /// the panel is accessed, the list of updates will be empty. It will be non empty
    /// only if the user asks for an explicit refresh or after the first background
    /// task sweep.
    /// </summary>
    [OrchardFeature("Gallery.Updates")]
    public class BackgroundPackageUpdateTask : IBackgroundTask {
        private readonly IPackageUpdateService _packageUpdateService;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IBackgroundPackageUpdateStatus _backgroundPackageUpdateStatus;

        public BackgroundPackageUpdateTask(IPackageUpdateService packageUpdateService, 
            IPackagingSourceManager packagingSourceManager, 
            IBackgroundPackageUpdateStatus backgroundPackageUpdateStatus) {

            _packageUpdateService = packageUpdateService;
            _packagingSourceManager = packagingSourceManager;
            _backgroundPackageUpdateStatus = backgroundPackageUpdateStatus;
        }

        public void Sweep() {
            _backgroundPackageUpdateStatus.Value = _packageUpdateService.GetPackagesStatus(_packagingSourceManager.GetSources());
        }
    }
}