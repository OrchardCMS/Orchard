using System;
using System.IO;
using System.Web.Hosting;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.UI.Notify;
using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageInstaller : IPackageInstaller {
        private const string PackagesPath = "packages";
        private const string SolutionFilename = "Orchard.sln";

        private readonly INotifier _notifier;
        private readonly IExtensionManager _extensionManager;

        public PackageInstaller(INotifier notifier, 
            IExtensionManager extensionManager) {
            _notifier = notifier;
            _extensionManager = extensionManager;

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
            string solutionPath;
            var installedPackagesPath = String.Empty;
            if (TryGetSolutionPath(applicationPath, out solutionPath)) {
                installedPackagesPath = Path.Combine(solutionPath, PackagesPath);
                try {
                    var packageManager = new NuGetPackageManager(
                        packageRepository,
                        new DefaultPackagePathResolver(location),
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
                ExtensionType = package.Id.StartsWith(PackagingSourceManager.ThemesPrefix) ? DefaultExtensionTypes.Theme : DefaultExtensionTypes.Module,
                ExtensionPath = applicationPath
            };
        }

        public void Uninstall(string packageId, string applicationPath) {
            // this logger is used to render NuGet's log on the notifier
            var logger = new NugetLogger(_notifier);

            string solutionPath;
            // if we can access the parent directory, and the solution is inside, NuGet-uninstall the package here
            if (TryGetSolutionPath(applicationPath, out solutionPath)) {
                var installedPackagesPath = Path.Combine(solutionPath, PackagesPath);
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
            } else {
                // otherwise delete the folder

                string extensionPath = packageId.StartsWith(PackagingSourceManager.ThemesPrefix)
                                           ? "~/Themes/" + packageId.Substring(PackagingSourceManager.ThemesPrefix.Length)
                                           : "~/Modules/" + packageId.Substring(PackagingSourceManager.ThemesPrefix.Length);

                string extensionFullPath = HostingEnvironment.MapPath(extensionPath);

                if (Directory.Exists(extensionFullPath)) {
                    Directory.Delete(extensionFullPath, true);
                }
                else {
                    throw new OrchardException(T("Package not found: ", packageId));
                }
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
    }
}