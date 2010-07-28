using System;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Packaging.Services;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class DefaultPackagingUpdater : IFeatureEventHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;

        public DefaultPackagingUpdater(IPackagingSourceManager packagingSourceManager) {
            _packagingSourceManager = packagingSourceManager;
        }

        public void Install(Feature feature) {
            // add http://orchardproject.net/feeds/modules as the default Modules Feed
            _packagingSourceManager.AddSource(new PackagingSource { Id = Guid.NewGuid(), FeedTitle = "Orchard Module Gallery", FeedUrl = "http://orchardproject.net/feeds/modules" });
        }

        public void Enable(Feature feature) {
        }

        public void Disable(Feature feature) {
        }

        public void Uninstall(Feature feature) {
        }
    }
}