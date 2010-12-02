using System;
using System.IO;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.UI.Notify;
using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageInstaller : IPackageInstaller {
        private const string PackagesPath = "packages";

        private readonly INotifier _notifier;
        private readonly IExtensionManager _extensionManager;
        private readonly IAppDataFolderRoot _appDataFolderRoot;

        public PackageInstaller(INotifier notifier, 
            IExtensionManager extensionManager,
            IAppDataFolderRoot appDataFolderRoot) {
            _notifier = notifier;
            _extensionManager = extensionManager;
            _appDataFolderRoot = appDataFolderRoot;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public PackageInfo Install(string packageId, string version, string location, string applicationPath) {
            // instantiates the appropriate package repository
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(location, "Default"));

            // gets an IPackage instance from the repository
            var packageVersion = String.IsNullOrEmpty(version) ? null : new Version(version);
            var package = packageRepository.FindPackage(packageId, packageVersion);
            if (package == null)
            {
                throw new ArgumentException(T("The specified package could not be found, id:{0} version:{1}", packageId, String.IsNullOrEmpty(version) ? T("No version").Text : version).Text);
            }

            return Install(package, packageRepository, location, applicationPath);
        }

        public PackageInfo Install(IPackage package, string location, string applicationPath) {
            // instantiates the appropriate package repository
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource(location, "Default"));
            return Install(package, packageRepository, location, applicationPath);
        }

        protected PackageInfo Install(IPackage package, IPackageRepository packageRepository, string location, string applicationPath) {
            // this logger is used to render NuGet's log on the notifier
            var logger = new NugetLogger(_notifier);

            bool installed = false;

            // if we can access the parent directory, and the solution is inside, NuGet-install the package here
            var installedPackagesPath = Path.Combine(_appDataFolderRoot.RootFolder, PackagesPath);

            try
            {
                var packageManager = new NuGetPackageManager(
                        packageRepository,
                        new DefaultPackagePathResolver(location),
                        new PhysicalFileSystem(installedPackagesPath) { Logger = logger }
                    ) { Logger = logger };

                packageManager.InstallPackage(package, ignoreDependencies: true);
                installed = true;
            }
            catch
            {
                // installing the package in the appdata folder failed
            }

            // if the package got installed successfully, use it, otherwise use the previous repository
            var sourceRepository = installed
                ? new LocalPackageRepository(installedPackagesPath)
                : packageRepository;

            var project = new FileBasedProjectSystem(applicationPath) { Logger = logger };
            var projectManager = new ProjectManager(
                sourceRepository, // source repository for the package to install
                new DefaultPackagePathResolver(applicationPath),
                project,
                new ExtensionReferenceRepository(project, sourceRepository, _extensionManager)
                ) { Logger = logger };

            // add the package to the project
            projectManager.AddPackageReference(package.Id, package.Version);

            return new PackageInfo
            {
                ExtensionName = package.Title ?? package.Id,
                ExtensionVersion = package.Version.ToString(),
                ExtensionType = package.Id.StartsWith("Orchard.Theme") ? DefaultExtensionTypes.Theme : DefaultExtensionTypes.Module,
                ExtensionPath = applicationPath
            };
        }

        public void Uninstall(string packageId, string applicationPath) {
            // this logger is used to render NuGet's log on the notifier
            var logger = new NugetLogger(_notifier);

            var installedPackagesPath = Path.Combine(_appDataFolderRoot.RootFolder, PackagesPath);
            var sourcePackageRepository = new LocalPackageRepository(installedPackagesPath);
            var project = new FileBasedProjectSystem(applicationPath) { Logger = logger };
            var projectManager = new ProjectManager(
                sourcePackageRepository,
                new DefaultPackagePathResolver(installedPackagesPath),
                project,
                new ExtensionReferenceRepository(project, sourcePackageRepository, _extensionManager)
                ) { Logger = logger };

            // add the package to the project
            projectManager.RemovePackageReference(packageId);

            var packageManager = new NuGetPackageManager(
                    sourcePackageRepository,
                    new DefaultPackagePathResolver(applicationPath),
                    new PhysicalFileSystem(installedPackagesPath) { Logger = logger }
                ) { Logger = logger };

            packageManager.UninstallPackage(packageId);
        }
    }
}