using System;
using System.IO;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.Localization;

using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageExpander : IPackageExpander {

        public PackageExpander() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public PackageInfo ExpandPackage(string packageId, string version, string location, string solutionFolder) {

            var packagesPath = Path.Combine(solutionFolder, "packages"); // where to download/uncompress packages
            var projectPath = Path.Combine(solutionFolder, "Orchard.Web"); // where to install packages (in the project's folder)

            // instantiates the appropriate package repository
            var packageRepository = Uri.IsWellFormedUriString(location, UriKind.Absolute)
                ? new DataServicePackageRepository(new Uri(location))
                : new LocalPackageRepository(location) as IPackageRepository;

            // gets an IPackage instance from the repository
            var packageVersion = String.IsNullOrEmpty(version) ? null : new Version(version);
            var package = packageRepository.FindPackage(packageId, packageVersion);

            if ( package == null ) {
                throw new ArgumentException(T("The specified package could not be found, id:{0} version:{1}", packageId, String.IsNullOrEmpty(version) ? T("No version").Text : version).Text);
            }

            var packageManager = new NuGetPackageManager(
                    packageRepository,
                    new DefaultPackagePathResolver(location),
                    new FileBasedProjectSystem(packagesPath)
                );

            // specifically tells to ignore dependencies
            packageManager.InstallPackage(package, ignoreDependencies: true);

            var projectManager = new ProjectManager(
                new LocalPackageRepository(packagesPath), // source repository for the package to install
                new DefaultPackagePathResolver(location),
                new FileBasedProjectSystem(projectPath), // the location of the project (where to copy the content files)
                new LocalPackageRepository(packagesPath) // the location of the uncompressed package, used to check if the package is already installed
                );

            // add the package to the project
            projectManager.AddPackageReference(packageId, packageVersion);


            return new PackageInfo {
                ExtensionName = package.Title,
                ExtensionVersion = package.Version.ToString(),
                ExtensionType = String.Empty, // todo: assign value
                ExtensionPath = projectPath
            };
        }

    }
}