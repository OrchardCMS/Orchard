using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NuGet;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;

using NuGetPackageManager = NuGet.PackageManager;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageExpander : IPackageExpander {
        private const string ContentTypePrefix = "Orchard ";
        private readonly IVirtualPathProvider _virtualPathProvider;

        public PackageExpander(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public PackageInfo ExpandPackage(string packageId, string version, string location, string destination) {
            var context = new ExpandContext();

            var packagesPath = Path.Combine(destination, "packages");
            var projectPath = Path.Combine(destination, "Orchard.Web");

            BeginPackage(context, packageId, version, location, packagesPath);
            try {

                var packageRepository = Uri.IsWellFormedUriString(location, UriKind.Absolute)
                    ? new DataServicePackageRepository(new Uri(location))
                    : new LocalPackageRepository(location) as IPackageRepository;

                var package = packageRepository.FindPackage(packageId, exactVersion: new Version(version));
                
                if(package == null) {
                    throw new ArgumentException(T("The specified package could not be found: {0}.{1}", packageId, version).Text);
                }

                context.ExtensionName = package.Title;
                context.ExtensionVersion = package.Version.ToString();
                context.TargetPath = projectPath;

                // packageManager.InstallPackage(package, ignoreDependencies: true);

                //var packageManager = new NuGetPackageManager(
                //    packageRepository,
                //    new DefaultPackagePathResolver(location),
                //    new FileBasedProjectSystem(packagesPath)
                //);

                var projectManager = new ProjectManager(
                    packageRepository, // source repository for the package to install
                    new DefaultPackagePathResolver(location), 
                    new FileBasedProjectSystem(projectPath), // the location of the project (where to copy the content files)
                    new LocalPackageRepository(packagesPath) // the location of the uncompressed package, used to check if the package is already installed
                    );

                // add the package to the project
                projectManager.AddPackageReference(packageId, new Version(version));


#if REFACTORING
                GetCoreProperties(context);
                EstablishPaths(context, _virtualPathProvider);

                string projectFile = context.ExtensionName + ".csproj";
                if (LoadProject(context, projectFile)) {
                    ExtractFile(context, projectFile);
                    ExtractProjectFiles(context, "Compile", "Content", "None", "EmbeddedResource");
                    ExtractReferenceFiles(context);
                }
                else if (context.ExtensionType == "Theme") {
                    // this is a simple theme with no csproj
                    ExtractThemeFiles(context);
                }
#endif
            }
            finally {
                EndPackage(context);
            }

            return new PackageInfo {
                ExtensionName = context.ExtensionName,
                ExtensionVersion = context.ExtensionVersion,
                ExtensionType = context.ExtensionType,
                ExtensionPath = context.TargetPath
            };
        }

        private void ExtractFile(ExpandContext context, string relativePath) {
#if REFACTORING
            Uri partUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            PackagePart packagePart = context.Package.GetPart(partUri);
            using (Stream packageStream = packagePart.GetStream(FileMode.Open, FileAccess.Read)) {
                string filePath = _virtualPathProvider.Combine(context.TargetPath, relativePath);
                string folderPath = _virtualPathProvider.GetDirectoryName(filePath);
                if (!_virtualPathProvider.DirectoryExists(folderPath)) {
                    _virtualPathProvider.CreateDirectory(folderPath);
                }

                using (Stream fileStream = _virtualPathProvider.CreateFile(filePath)) {
                    packageStream.CopyTo(fileStream);
                }
            }
#endif
        }

        private void ExtractProjectFiles(ExpandContext context, params string[] itemGroupTypes) {
            IEnumerable<XElement> itemGroups = context.Project
                .Elements(Ns("Project"))
                .Elements(Ns("ItemGroup"));

            foreach (string itemGroupType in itemGroupTypes) {
                IEnumerable<string> includePaths = itemGroups
                    .Elements(Ns(itemGroupType))
                    .Attributes("Include")
                    .Select(x => x.Value);
                foreach (string includePath in includePaths) {
                    ExtractFile(context, includePath);
                }
            }
        }

        private void ExtractReferenceFiles(ExpandContext context) {
            var entries = context.Project
                .Elements(Ns("Project"))
                .Elements(Ns("ItemGroup"))
                .Elements(Ns("Reference"))
                .Select(reference => new {
                    Include = reference.Attribute("Include"),
                    HintPath = reference.Element(Ns("HintPath"))
                })
                .Where(entry => entry.Include != null);

            foreach (var entry in entries) {
                var assemblyName = new AssemblyName(entry.Include.Value);
                string hintPath = entry.HintPath != null ? entry.HintPath.Value : null;

                string virtualPath = "bin/" + assemblyName.Name + ".dll";
                if (PartExists(context, virtualPath)) {
                    ExtractFile(context, virtualPath);
                }
                else if (hintPath != null) {}
            }
        }

        private void ExtractThemeFiles(ExpandContext context) {
#if REFACTORING
            foreach (var relativePath in from p in context.Package.GetParts()
                                         where p.Uri.ToString().StartsWith("/" + context.ExtensionName + "/", StringComparison.OrdinalIgnoreCase)
                                         select p.Uri.ToString().Substring(("/" + context.ExtensionName + "/").Length)) {
                ExtractFile(context, relativePath);
            }
#endif
        }

        private bool PartExists(ExpandContext context, string relativePath) {
#if REFACTORING
            Uri projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            return context.Package.PartExists(projectUri);
#else
            return false;
#endif
        }


        private XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }

        private static bool LoadProject(ExpandContext context, string relativePath) {
#if REFACTORING
            Uri projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            if (!context.Package.PartExists(projectUri)) {
                return false;
            }
            PackagePart part = context.Package.GetPart(projectUri);
            using (Stream stream = part.GetStream(FileMode.Open, FileAccess.Read)) {
                context.Project = XDocument.Load(stream);
            }
#endif
            return true;
        }

        private void BeginPackage(ExpandContext context, string packageId, string version, string location, string destination) {

#if REFACTORING
            context.Package = Package.Open(context.Stream, FileMode.Open, FileAccess.Read);
#endif
        }

        private void EndPackage(ExpandContext context) {
#if REFACTORING
            context.Package.Close();
#endif
        }

        private void GetCoreProperties(ExpandContext context) {
#if REFACTORING
            context.ExtensionName = context.Package.PackageProperties.Identifier;
            context.ExtensionVersion = context.Package.PackageProperties.Version;

            string contentType = context.Package.PackageProperties.ContentType;
            if (contentType.StartsWith(ContentTypePrefix)) {
                context.ExtensionType = contentType.Substring(ContentTypePrefix.Length);
            }
#endif
        }

        private void EstablishPaths(ExpandContext context, IVirtualPathProvider virtualPathProvider) {
            context.SourcePath = "\\" + context.ExtensionName + "\\";
            switch (context.ExtensionType) {
                case "Theme":
                    context.TargetPath = virtualPathProvider.Combine("~/Themes/" + context.ExtensionName);
                    break;
                case "Module":
                    context.TargetPath = virtualPathProvider.Combine("~/Modules/" + context.ExtensionName);
                    break;
                default:
                    throw new OrchardCoreException(T("Unknown extension type \"{0}\"", context.ExtensionType));
            }
        }

        #region Nested type: ExpandContext

        private class ExpandContext {
            public Stream Stream { get; set; }
            //public Package Package { get; set; }

            public string ExtensionName { get; set; }
            public string ExtensionVersion { get; set; }
            public string ExtensionType { get; set; }

            public string TargetPath { get; set; }

            public string SourcePath { get; set; }

            public XDocument Project { get; set; }
        }

        #endregion
    }
}