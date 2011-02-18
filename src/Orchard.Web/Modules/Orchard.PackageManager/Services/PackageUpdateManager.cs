using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Services;
using Orchard.UI.Notify;

namespace Orchard.PackageManager.Services {
    public class PackagesStatusResult {
        public IEnumerable<UpdatePackageEntry> Entries { get; set; }
        public IEnumerable<Exception> Errors { get; set; }
    }

    public class UpdatePackageEntry {
        public ExtensionDescriptor ExtensionsDescriptor { get; set; }
        public IList<PackagingEntry> PackageVersions { get; set; }

        /// <summary>
        /// Return version to install if out-of-date, null otherwise.
        /// </summary>
        public PackagingEntry NewVersionToInstall {
            get {
                PackagingEntry updateToVersion = null;
                var latestUpdate = this.PackageVersions.OrderBy(v => new Version(v.Version)).Last();
                if (new Version(latestUpdate.Version) > new Version(this.ExtensionsDescriptor.Version)) {
                    updateToVersion = latestUpdate;
                }
                return updateToVersion;
            }
        }
    }

    public interface IPackageUpdateService : IDependency {
        PackagesStatusResult GetPackagesStatus(IEnumerable<PackagingSource> sources);
        void TriggerRefresh();
        void Update(PackagingEntry entry);
        void Uninstall(string packageId);
    }

    public class PackageUpdateService : IPackageUpdateService {
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IExtensionManager _extensionManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private readonly INotifier _notifier;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IPackageManager _packageManager;
        private readonly IFolderUpdater _folderUpdater;

        public PackageUpdateService(IPackagingSourceManager packagingSourceManager,
            IExtensionManager extensionManager,
            ICacheManager cacheManager,
            IClock clock,
            ISignals signals,
            INotifier notifier,
            IVirtualPathProvider virtualPathProvider,
            IPackageManager packageManager,
            IFolderUpdater folderUpdater) {

            _packagingSourceManager = packagingSourceManager;
            _extensionManager = extensionManager;
            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;
            _notifier = notifier;
            _virtualPathProvider = virtualPathProvider;
            _packageManager = packageManager;
            _folderUpdater = folderUpdater;
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
                result.Entries = result.Entries.Concat(sourceResult.Entries);
                result.Errors = result.Errors.Concat(sourceResult.Errors);
            }

            return result;
        }

        public void TriggerRefresh() {
            _signals.Trigger("PackageUpdateService");
        }

        private PackagesStatusResult GetPackages(PackagingSource packagingSource) {
            return _cacheManager.Get(packagingSource.FeedUrl, ctx => {
                // Refresh every minute or when signal was triggered
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(5)));
                ctx.Monitor(_signals.When("PackageUpdateService"));

                // We cache exception because we are calling on a network feed, and failure may
                // take quite some time.
                var result = new PackagesStatusResult {
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

            var packages = _packagingSourceManager.GetExtensionList(packagingSource)
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

        public class UpdateContext {
            public PackagingEntry PackagingEntry { get; set; }
            public bool IsTheme {
                get {
                    return PackagingEntry.PackageId.StartsWith(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme));
                }
            }
            public string ExtensionFolder {
                get { return IsTheme ? "Themes" : "Modules"; }
            }
            public string ExtensionId {
                get {
                    return IsTheme ?
                        PackagingEntry.PackageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length) :
                        PackagingEntry.PackageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length);
                }
            }
        }

        public void Update(PackagingEntry entry) {
            var context = new UpdateContext { PackagingEntry = entry };

            // 1. Backup extension folder
            try {
                BackupExtensionFolder(context.ExtensionFolder, context.ExtensionId);
            }
            catch (Exception exception) {
                throw new OrchardException(T("Unable to backup existing local package directory."), exception);
            }

            // 2. If extension is installed, need to un-install first
            try {
                UninstallExtensionIfNeeded(context);
            }
            catch (Exception exception) {
                throw new OrchardException(T("Unable to un-install local package before updating."), exception);
            }

            // 3. Install package from Gallery to temporary folder
            DirectoryInfo newPackageFolder;
            try {
                newPackageFolder = InstallPackage(context);
            }
            catch (Exception exception) {
                throw new OrchardException(T("Package installation failed."), exception);
            }

            // 4. Copy new package content to extension folder
            try {
                UpdateExtensionFolder(context, newPackageFolder);
            }
            catch (Exception exception) {
                throw new OrchardException(T("Package update failed."), exception);
            }
        }

        public void Uninstall(string packageId) {
            var context = new UpdateContext { PackagingEntry = new PackagingEntry {PackageId = packageId} };

            // Backup extension folder
            try {
                BackupExtensionFolder(context.ExtensionFolder, context.ExtensionId);
            }
            catch (Exception exception) {
                throw new OrchardException(T("Unable to backup existing local package directory."), exception);
            }

            // Uninstall package from local folder
            _packageManager.Uninstall(packageId, _virtualPathProvider.MapPath("~/"));
            _notifier.Information(T("Successfully un-installed local package {0}", packageId));
        }

        private void BackupExtensionFolder(string extensionFolder, string extensionId) {
            var tempPath = _virtualPathProvider.Combine("~", extensionFolder, "_Backup", extensionId);
            string localTempPath = null;
            for (int i = 0; i < 1000; i++) {
                localTempPath = _virtualPathProvider.MapPath(tempPath) + (i == 0 ? "" : "." + i.ToString());
                if (!Directory.Exists(localTempPath)) {
                    Directory.CreateDirectory(localTempPath);
                    break;
                }
                localTempPath = null;
            }

            if (localTempPath == null) {
                throw new OrchardException(T("Backup folder {0} has too many backups subfolder (limit is 1,000)", tempPath));
            }

            var backupFolder = new DirectoryInfo(localTempPath);
            var source = new DirectoryInfo(_virtualPathProvider.MapPath(_virtualPathProvider.Combine("~", extensionFolder, extensionId)));
            _folderUpdater.Backup(source, backupFolder);
            _notifier.Information(T("Successfully backed up local package to local folder \"{0}\"", backupFolder));
        }

        private void UninstallExtensionIfNeeded(UpdateContext context) {
            // Nuget requires to un-install the currently installed packages if the new
            // package is the same version or an older version
            var extension = _extensionManager
                .AvailableExtensions()
                .Where(e => e.Id == context.ExtensionId && new Version(e.Version) >= new Version(context.PackagingEntry.Version))
                .FirstOrDefault();
            if (extension == null)
                return;

            _packageManager.Uninstall(context.PackagingEntry.PackageId, _virtualPathProvider.MapPath("~/"));
            _notifier.Information(T("Successfully un-installed local package {0}", context.ExtensionId));
        }

        private DirectoryInfo InstallPackage(UpdateContext context) {
            var tempPath = _virtualPathProvider.Combine("~", context.ExtensionFolder, "_Updates");
            var destPath = _virtualPathProvider.Combine(tempPath, context.ExtensionFolder, context.ExtensionId);
            var localDestPath = (_virtualPathProvider.MapPath(destPath));
            if (Directory.Exists(localDestPath)) {
                Directory.Delete(localDestPath, true);
            }
            _packageManager.Install(context.PackagingEntry.PackageId, context.PackagingEntry.Version, context.PackagingEntry.Source.FeedUrl, _virtualPathProvider.MapPath(tempPath));
            return new DirectoryInfo(localDestPath);
        }

        private void UpdateExtensionFolder(UpdateContext context, DirectoryInfo newPackageFolder) {
            var extensionPath = _virtualPathProvider.Combine("~", context.ExtensionFolder, context.ExtensionId);
            var extensionFolder = new DirectoryInfo(_virtualPathProvider.MapPath(extensionPath));

            _folderUpdater.Update(extensionFolder, newPackageFolder);

            _notifier.Information(T("Successfully installed package \"{0}\" to local folder \"{1}\"", context.ExtensionId, extensionFolder));
        }
    }
}