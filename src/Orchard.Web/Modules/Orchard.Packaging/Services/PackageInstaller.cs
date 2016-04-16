using System;
using System.IO;
using NuGet;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Extensions;
using Orchard.Packaging.Models;
using Orchard.Services;
using Orchard.UI;
using Orchard.UI.Notify;
using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageInstaller : IPackageInstaller {
        private const string PackagesPath = "packages";
        private const string SolutionFilename = "Orchard.sln";

        private readonly INotifier _notifier;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IExtensionManager _extensionManager;
        private readonly IFolderUpdater _folderUpdater;
        private readonly IClock _clock;

        public PackageInstaller(
            INotifier notifier,
            IVirtualPathProvider virtualPathProvider,
            IExtensionManager extensionManager,
            IFolderUpdater folderUpdater,
            IClock clock) {

            _notifier = notifier;
            _virtualPathProvider = virtualPathProvider;
            _extensionManager = extensionManager;
            _folderUpdater = folderUpdater;
            _clock = clock;

            T = NullLocalizer.Instance;
            Logger = Logging.NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public Logging.ILogger Logger { get; set;  }

        public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
            // instantiates the appropriate package repository
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(location, "Default"));

            // gets an IPackage instance from the repository
            var packageVersion = String.IsNullOrEmpty(version) ? null : new Version(version);
            var package = packageRepository.FindPackage(packageId, packageVersion);
            if (package == null) {
                throw new ArgumentException(T("The specified package could not be found, id:{0} version:{1}", packageId, String.IsNullOrEmpty(version) ? T("No version").Text : version).Text);
            }

            return InstallPackage(package, packageRepository, location, applicationPath);
        }

        public PackageInfo Install(IPackage package, string location, string applicationPath) {
            // instantiates the appropriate package repository
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(location, "Default"));
            return InstallPackage(package, packageRepository, location, applicationPath);
        }

        protected PackageInfo InstallPackage(IPackage package, IPackageRepository packageRepository, string location, string applicationPath) {
            bool previousInstalled;

            // 1. See if extension was previous installed and backup its folder if so
            try {
                previousInstalled = BackupExtensionFolder(package.ExtensionFolder(), package.ExtensionId());
            }
            catch (Exception exception) {
                throw new OrchardException(T("Unable to backup existing local package directory."), exception);
            }

            if (previousInstalled) {
                // 2. If extension is installed, need to un-install first
                try {
                    UninstallExtensionIfNeeded(package);
                }
                catch (Exception exception) {
                    throw new OrchardException(T("Unable to un-install local package before updating."), exception);
                }
            }

            var packageInfo = ExecuteInstall(package, packageRepository, location, applicationPath);

            // check the new package is compatible with current Orchard version
            var descriptor = package.GetExtensionDescriptor(packageInfo.ExtensionType);

            if(descriptor != null) {
                if(new FlatPositionComparer().Compare(descriptor.OrchardVersion, typeof(ContentItem).Assembly.GetName().Version.ToString()) >= 0) {
                    if (previousInstalled) {
                        // restore the previous version
                        RestoreExtensionFolder(package.ExtensionFolder(), package.ExtensionId());
                    }
                    else {
                        // just uninstall the new package
                        Uninstall(package.Id, _virtualPathProvider.MapPath("~\\"));
                    }

                    Logger.Error(String.Format("The package is compatible with version {0} and above. Please update Orchard or install another version of this package.", descriptor.OrchardVersion));
                    throw new OrchardException(T("The package is compatible with version {0} and above. Please update Orchard or install another version of this package.", descriptor.OrchardVersion));
                }    
            }

            return packageInfo;
        }

        /// <summary>
        /// Executes a package installation.
        /// </summary>
        /// <param name="package">The package to install.</param>
        /// <param name="packageRepository">The repository for the package.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="targetPath">The path where to install the package.</param>
        /// <returns>The package information.</returns>
        protected PackageInfo ExecuteInstall(IPackage package, IPackageRepository packageRepository, string sourceLocation, string targetPath) {
            // this logger is used to render NuGet's log on the notifier
            var logger = new NugetLogger(_notifier);

            bool installed = false;

            // if we can access the parent directory, and the solution is inside, NuGet-install the package here
            string solutionPath;
            var installedPackagesPath = String.Empty;
            if (TryGetSolutionPath(targetPath, out solutionPath)) {
                installedPackagesPath = Path.Combine(solutionPath, PackagesPath);
                try {
                    var packageManager = new NuGetPackageManager(
                        packageRepository,
                        new DefaultPackagePathResolver(sourceLocation),
                        new PhysicalFileSystem(installedPackagesPath) {Logger = logger}
                        ) {Logger = logger};

                    packageManager.InstallPackage(package, true);
                    installed = true;
                }
                catch {
                    // installing the package at the solution level failed
                }
            }

            // if the package got installed successfully, use it, otherwise use the previous repository
            var sourceRepository = installed
                ? new LocalPackageRepository(installedPackagesPath)
                : packageRepository;

            var project = new FileBasedProjectSystem(targetPath) { Logger = logger };
            project.OverwriteLastWriteTimeUtcForAddedFiles(_clock.UtcNow);
            var projectManager = new ProjectManager(
                sourceRepository, // source repository for the package to install
                new DefaultPackagePathResolver(targetPath),
                project,
                new ExtensionReferenceRepository(project, sourceRepository, _extensionManager)
                ) { Logger = logger };

            // add the package to the project
            projectManager.AddPackageReference(package.Id, package.Version);

            return new PackageInfo {
                ExtensionName = package.Title ?? package.Id,
                ExtensionVersion = package.Version.ToString(),
                ExtensionType = package.Id.StartsWith(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme)) ? DefaultExtensionTypes.Theme : DefaultExtensionTypes.Module,
                ExtensionPath = targetPath
            };
        }

        /// <summary>
        /// Uninstalls a package.
        /// </summary>
        /// <param name="packageId">The package identifier for the package to be uninstalled.</param>
        /// <param name="applicationPath">The application path.</param>
        public void Uninstall(string packageId, string applicationPath) {
            string solutionPath;
            string extensionFullPath = string.Empty;

            if (packageId.StartsWith(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme))) {
                extensionFullPath = _virtualPathProvider.MapPath("~/Themes/" + packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length));
            } else if (packageId.StartsWith(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module))) {
                extensionFullPath = _virtualPathProvider.MapPath("~/Modules/" + packageId.Substring(PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length));
            }

            if (string.IsNullOrEmpty(extensionFullPath) ||
                !Directory.Exists(extensionFullPath)) {

                throw new OrchardException(T("Package not found: {0}", packageId));
            }

            // if we can access the parent directory, and the solution is inside, NuGet-uninstall the package here
            if (TryGetSolutionPath(applicationPath, out solutionPath)) {

                // this logger is used to render NuGet's log on the notifier
                var logger = new NugetLogger(_notifier);

                var installedPackagesPath = Path.Combine(solutionPath, PackagesPath);
                var sourcePackageRepository = new LocalPackageRepository(installedPackagesPath);

                try {
                    var project = new FileBasedProjectSystem(applicationPath) {Logger = logger};
                    var projectManager = new ProjectManager(
                        sourcePackageRepository,
                        new DefaultPackagePathResolver(installedPackagesPath),
                        project,
                        new ExtensionReferenceRepository(project, sourcePackageRepository, _extensionManager)
                        ) {Logger = logger};

                    // add the package to the project
                    projectManager.RemovePackageReference(packageId);
                }
                catch {
                    // Uninstalling the package at the solution level failed
                }

                try {
                    var packageManager = new NuGetPackageManager(
                        sourcePackageRepository,
                        new DefaultPackagePathResolver(applicationPath),
                        new PhysicalFileSystem(installedPackagesPath) {Logger = logger}
                        ) {Logger = logger};

                    packageManager.UninstallPackage(packageId);
                }
                catch {
                    // Package doesnt exist anymore
                }
            }

            // If the package was not installed through nuget we still need to try to uninstall it by removing its directory
            if (Directory.Exists(extensionFullPath)) {
                Directory.Delete(extensionFullPath, true);
            }
        }

        private static bool TryGetSolutionPath(string applicationPath, out string parentPath) {
            try {
                parentPath = Directory.GetParent(applicationPath).Parent.FullName;
                var solutionPath = Path.Combine(parentPath, SolutionFilename);
                return File.Exists(solutionPath);
            }
            catch {
                // Either solution does not exist or we are running under medium trust
                parentPath = null;
                return false;
            }
        }

        private bool RestoreExtensionFolder(string extensionFolder, string extensionId) {
            var source = new DirectoryInfo(_virtualPathProvider.MapPath(_virtualPathProvider.Combine("~", extensionFolder, extensionId)));

            if (source.Exists) {
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
                _folderUpdater.Restore(backupFolder, source);
                _notifier.Success(T("Successfully restored local package to local folder \"{0}\"", source));

                return true;
            }

            return false;
        }

        private bool BackupExtensionFolder(string extensionFolder, string extensionId) {
            var source = new DirectoryInfo(_virtualPathProvider.MapPath(_virtualPathProvider.Combine("~", extensionFolder, extensionId)));

            if (source.Exists) {
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
                _folderUpdater.Backup(source, backupFolder);
                _notifier.Success(T("Successfully backed up local package to local folder \"{0}\"", backupFolder));

                return true;
            }

            return false;
        }

        private void UninstallExtensionIfNeeded(IPackage package) {
            // Nuget requires to un-install the currently installed packages if the new
            // package is the same version or an older version
            try {
                Uninstall(package.Id, _virtualPathProvider.MapPath("~\\"));
                _notifier.Success(T("Successfully un-installed local package {0}", package.ExtensionId()));
            }
            catch {}
        }
    }
}