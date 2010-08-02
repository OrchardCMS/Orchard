using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization;

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

        #region IPackageExpander Members

        public PackageInfo ExpandPackage(Stream packageStream) {
            var context = new ExpandContext();
            BeginPackage(context, packageStream);
            try {
                GetCoreProperties(context);
                EstablishPaths(context, _virtualPathProvider);

                string projectFile = context.ExtensionName + ".csproj";
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

        #endregion

        private void ExtractFile(ExpandContext context, string relativePath) {
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

        private bool PartExists(ExpandContext context, string relativePath) {
            Uri projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            return context.Package.PartExists(projectUri);
        }


        private XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }

        private static bool LoadProject(ExpandContext context, string relativePath) {
            Uri projectUri = PackUriHelper.CreatePartUri(new Uri(context.SourcePath + relativePath, UriKind.Relative));
            if (!context.Package.PartExists(projectUri)) {
                return false;
            }
            PackagePart part = context.Package.GetPart(projectUri);
            using (Stream stream = part.GetStream(FileMode.Open, FileAccess.Read)) {
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

            string contentType = context.Package.PackageProperties.ContentType;
            if (contentType.StartsWith(ContentTypePrefix)) {
                context.ExtensionType = contentType.Substring(ContentTypePrefix.Length);
            }
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
            public Package Package { get; set; }

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