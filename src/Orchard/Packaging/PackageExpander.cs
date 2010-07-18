using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Packaging {
    public class PackageExpander : IPackageExpander {
        private const string ContentTypePrefix = "Orchard ";
        private readonly IVirtualPathProvider _virtualPathProvider;

        public PackageExpander(
            IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;
        }

        class ExpandContext {
            public Stream Stream { get; set; }
            public Package Package { get; set; }

            public string ExtensionName { get; set; }
            public string ExtensionVersion { get; set; }
            public string ExtensionType { get; set; }

            public string TargetPath { get; set; }

            public string SourcePath { get; set; }

            public XDocument Project { get; set; }
        }

        public PackageInfo ExpandPackage(Stream packageStream) {
            var context = new ExpandContext();
            BeginPackage(context, packageStream);
            try {
                GetCoreProperties(context);
                EstablishPaths(context, _virtualPathProvider);

                var projectFile = context.ExtensionName + ".csproj";
                if (LoadProject(context, projectFile)) {
                    ExtractFile(context, projectFile);
                    ExtractProjectFiles(context, "Compile", "Content", "None", "EmbeddedResource");
                    ExtractReferenceFiles(context);
                }
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
            var partUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            var packagePart = context.Package.GetPart(partUri);
            using (var packageStream = packagePart.GetStream(FileMode.Open, FileAccess.Read)) {
                var filePath = Path.Combine(context.TargetPath, relativePath);
                var folderPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    packageStream.CopyTo(fileStream);
                }
            }
        }

        private void ExtractProjectFiles(ExpandContext context, params string[] itemGroupTypes) {
            var itemGroups = context.Project
                .Elements(Ns("Project"))
                .Elements(Ns("ItemGroup"));

            foreach (var itemGroupType in itemGroupTypes) {
                var includePaths = itemGroups
                    .Elements(Ns(itemGroupType))
                    .Attributes("Include")
                    .Select(x => x.Value);
                foreach (var includePath in includePaths) {
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
                var hintPath = entry.HintPath != null ? entry.HintPath.Value : null;

                var virtualPath = "bin/" + assemblyName.Name + ".dll";
                if (PartExists(context, virtualPath)) {
                    ExtractFile(context, virtualPath);
                }
                else if (hintPath != null) {
                }
            }
        }

        private bool PartExists(ExpandContext context, string relativePath) {
            var projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            return context.Package.PartExists(projectUri);
        }


        private XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }
        private static bool LoadProject(ExpandContext context, string relativePath) {
            var projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            if (!context.Package.PartExists(projectUri))
                return false;
            var part = context.Package.GetPart(projectUri);
            using (var stream = part.GetStream(FileMode.Open, FileAccess.Read)) {
                context.Project = XDocument.Load(stream);
            }
            return true;
        }

        private void BeginPackage(ExpandContext context, Stream packageStream) {
            if (packageStream.CanSeek) {
                context.Stream = packageStream;
            }
            else {
                context.Stream = new MemoryStream();
                packageStream.CopyTo(context.Stream);
            }
            context.Package = Package.Open(context.Stream, FileMode.Open, FileAccess.Read);
        }

        private void EndPackage(ExpandContext context) {
            context.Package.Close();
        }

        private void GetCoreProperties(ExpandContext context) {
            context.ExtensionName = context.Package.PackageProperties.Identifier;
            context.ExtensionVersion = context.Package.PackageProperties.Version;

            var contentType = context.Package.PackageProperties.ContentType;
            if (contentType.StartsWith(ContentTypePrefix))
                context.ExtensionType = contentType.Substring(ContentTypePrefix.Length);
        }

        private void EstablishPaths(ExpandContext context, IVirtualPathProvider virtualPathProvider) {
            context.SourcePath = "\\" + context.ExtensionName + "\\";
            if (context.ExtensionType == "Theme") {
                context.TargetPath = virtualPathProvider.MapPath("~/Themes-temp/" + context.ExtensionName);
            }
            else if (context.ExtensionType == "Module") {
                context.TargetPath = virtualPathProvider.MapPath("~/Modules-temp/" + context.ExtensionName);
            }
            else {
                throw new ApplicationException("Unknown extension type");
            }
        }
    }
}