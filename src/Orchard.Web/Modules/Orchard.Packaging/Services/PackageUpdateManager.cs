using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Services;

namespace Orchard.Packaging.Services {
    public interface IPackageUpdateService : IDependency {
        PackagesStatusResult GetPackagesStatus(IEnumerable<PackagingSource> sources);
        void TriggerRefresh();
    }

    [OrchardFeature("Gallery.Updates")]
    public class PackageUpdateService : IPackageUpdateService {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;

        public PackageUpdateService(IPackagingSourceManager packagingSourceManager,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            IClock clock,
            ISignals signals) {

            _packagingSourceManager = packagingSourceManager;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public PackagesStatusResult GetPackagesStatus(IEnumerable<PackagingSource> sources) {
            var result = new PackagesStatusResult {
                Entries = new List<UpdatePackageEntry>(),
                Errors = new List<Exception>()
            };

            foreach (var source in sources) {
                var sourceResult = GetPackages(source);
                result.DateTimeUtc = sourceResult.DateTimeUtc;
                result.Entries = result.Entries.Concat(sourceResult.Entries);
                result.Errors = result.Errors.Concat(sourceResult.Errors);
            }

            return result;
        }

        public void TriggerRefresh() {
            _signals.Trigger("PackageUpdateService");
        }

        private PackagesStatusResult GetPackages(PackagingSource packagingSource) {
            // Refresh every 23 hours or when signal was triggered
            return _cacheManager.Get(packagingSource.FeedUrl, ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(60 * 23)));
                ctx.Monitor(_signals.When("PackageUpdateService"));

                // We cache exception because we are calling on a network feed, and failure may
                // take quite some time.
                var result = new PackagesStatusResult {
                    DateTimeUtc = _clock.UtcNow,
                    Entries = new List<UpdatePackageEntry>(),
                    Errors = new List<Exception>()
                };
                try {
                    result.Entries = GetPackagesWorker(packagingSource);
                }
                catch (Exception e) {
                    result.Errors = new[] { e };
                }
                return result;
            });
        }

        private IEnumerable<UpdatePackageEntry> GetPackagesWorker(PackagingSource packagingSource) {
            var list = new Dictionary<string, UpdatePackageEntry>(StringComparer.OrdinalIgnoreCase);

            var extensions = _extensionManager.AvailableExtensions();
            foreach (var extension in extensions) {
                var packageId = PackageBuilder.BuildPackageId(extension.Id, extension.ExtensionType);

                GetOrAddEntry(list, packageId).ExtensionsDescriptor = extension;
            }

            var packages = _packagingSourceManager.GetExtensionList(false, packagingSource)
                .ToList()
                .GroupBy(p => p.PackageId, StringComparer.OrdinalIgnoreCase);

            foreach (var package in packages) {
                var entry = GetOrAddEntry(list, package.Key);
                entry.PackageVersions = entry.PackageVersions.Concat(package).ToList();
            }

            return list.Values.Where(e => e.ExtensionsDescriptor != null && e.PackageVersions.Any());
        }

        private UpdatePackageEntry GetOrAddEntry(Dictionary<string, UpdatePackageEntry> list, string packageId) {
            UpdatePackageEntry entry;
            if (!list.TryGetValue(packageId, out entry)) {
                entry = new UpdatePackageEntry { PackageVersions = new List<PackagingEntry>() };
                list.Add(packageId, entry);
            }
            return entry;
        }
    }
}