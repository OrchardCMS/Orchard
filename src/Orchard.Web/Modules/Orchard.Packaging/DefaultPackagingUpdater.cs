using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Services;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class DefaultPackagingUpdater : IFeatureEventHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;

        public DefaultPackagingUpdater(IPackagingSourceManager packagingSourceManager) {
            _packagingSourceManager = packagingSourceManager;
        }

        public Localizer T { get; set; }

        public void Install(Feature feature) {
            _packagingSourceManager.AddSource( "Orchard Extensions Gallery", "http://feed.nuget.org/ctp2/odata/v1" );
        }

        public void Enable(Feature feature) {
        }

        public void Disable(Feature feature) {
        }

        public void Uninstall(Feature feature) {
        }
    }
}