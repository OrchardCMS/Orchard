using System;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.UI.Notify;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class DefaultPackagingUpdater : IFeatureEventHandler {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly INotifier _notifier;

        public DefaultPackagingUpdater(IPackagingSourceManager packagingSourceManager, INotifier notifier) {
            _packagingSourceManager = packagingSourceManager;
            _notifier = notifier;
        }

        public Localizer T { get; set; }

        public void Install(Feature feature) {
            _packagingSourceManager.AddSource(new PackagingSource { Id = Guid.NewGuid(), FeedTitle = "Orchard Module Gallery", FeedUrl = "http://orchardproject.net/gallery08/feed" });
        }

        public void Enable(Feature feature) {
        }

        public void Disable(Feature feature) {
        }

        public void Uninstall(Feature feature) {
        }
    }
}