using System;
using System.IO;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageInstaller : IPackageInstaller {
        private const string PackagesPath = "packages";
        private const string ProjectPath = "Orchard.Web";

        private readonly INotifier _notifier;

        public PackageInstaller(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public PackageInfo Install(string packageId, string version, string location, string solutionFolder) {

            var packagesPath = Path.Combine(solutionFolder, PackagesPath); // where to download/uncompress packages
            var projectPath = Path.Combine(solutionFolder, ProjectPath); // where to install packages (in the project's folder)
            var logger = new NugetLogger(_notifier);

            // instantiates the appropriate package repository
            var packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource("Default", location));

            // gets an IPackage instance from the repository
            var packageVersion = String.IsNullOrEmpty(version) ? null : new Version(version);
            var package = packageRepository.FindPackage(packageId, packageVersion);

            if ( package == null ) {
                throw new ArgumentException(T("The specified package could not be found, id:{0} version:{1}", packageId, String.IsNullOrEmpty(version) ? T("No version").Text : version).Text);
            }

            var packageManager = new NuGetPackageManager(
                    packageRepository,
                    new DefaultPackagePathResolver(location),
                    new PhysicalFileSystem(packagesPath) { Logger = logger }
                ) {Logger = logger};

            // specifically tells to ignore dependencies
            packageManager.InstallPackage(package, ignoreDependencies: true);

            var projectManager = new ProjectManager(
                new LocalPackageRepository(packagesPath), // source repository for the package to install
                new DefaultPackagePathResolver(location),
                new FileBasedProjectSystem(projectPath) { Logger = logger } // the location of the project (where to copy the content files)
                ) {Logger = logger};

            // add the package to the project
            projectManager.AddPackageReference(packageId, packageVersion);

            return new PackageInfo {
                ExtensionName = package.Title ?? package.Id,
                ExtensionVersion = package.Version.ToString(),
                ExtensionType = String.Empty, // todo: assign value
                ExtensionPath = projectPath
            };
        }

        public void Uninstall(string packageId, string solutionFolder) {
            var packagesPath = Path.Combine(solutionFolder, PackagesPath); // where to download/uncompress packages
            var projectPath = Path.Combine(solutionFolder, ProjectPath); // where to install packages (in the project's folder)
            var logger = new NugetLogger(_notifier);

            // instantiates the appropriate package repository
            var packageRepository = PackageRepositoryFactory.Default.CreateRepository(new PackageSource("Default", packagesPath));

            var projectManager = new ProjectManager(
               new LocalPackageRepository(packagesPath),
               new DefaultPackagePathResolver(packagesPath),
               new FileBasedProjectSystem(projectPath) { Logger = logger }
               ) { Logger = logger };

            // removes the package from the project
            projectManager.RemovePackageReference(packageId);

            var packageManager = new NuGetPackageManager(
                    packageRepository,
                    new DefaultPackagePathResolver(packagesPath),
                    new PhysicalFileSystem(packagesPath) { Logger = logger }
                ) { Logger = logger };


            packageManager.UninstallPackage(packageId);


        }
    }
}