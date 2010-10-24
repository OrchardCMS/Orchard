using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageBuilder : IPackageBuilder {
        private readonly IExtensionManager _extensionManager;
        private readonly IWebSiteFolder _webSiteFolder;

        private static readonly string[] _ignoredThemeExtensions = new[] {
            "obj", "pdb", "exclude"
        };
        private static readonly string[] _ignoredThemePaths = new[] {
            "/obj/"
        };

        private static bool IgnoreFile(string filePath) {
            return String.IsNullOrEmpty(filePath) ||
                _ignoredThemePaths.Any(filePath.Contains) ||
                _ignoredThemeExtensions.Contains(Path.GetExtension(filePath) ?? "");
        }

        public PackageBuilder(IExtensionManager extensionManager, IWebSiteFolder webSiteFolder) {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
        }

        #region IPackageBuilder Members

        public Stream BuildPackage(ExtensionDescriptor extensionDescriptor) {
            var context = new CreateContext();
            BeginPackage(context);
            try {
                EstablishPaths(context, _webSiteFolder, extensionDescriptor.Location, extensionDescriptor.Name, extensionDescriptor.ExtensionType);
                SetCoreProperties(context, extensionDescriptor);

                string projectFile = extensionDescriptor.Name + ".csproj";
                if (LoadProject(context, projectFile)) {
                    EmbedVirtualFile(context, projectFile, MediaTypeNames.Text.Xml);
                    EmbedProjectFiles(context, "Compile", "Content", "None", "EmbeddedResource");
                    EmbedReferenceFiles(context);
                }
                else if (extensionDescriptor.ExtensionType == "Theme") {
                    // this is a simple theme with no csproj
                    EmbedThemeFiles(context);
                }
            }
            finally {
                EndPackage(context);
            }

            if (context.Stream.CanSeek) {
                context.Stream.Seek(0, SeekOrigin.Begin);
            }

            return context.Stream;
        }

        #endregion

        private void SetCoreProperties(CreateContext context, ExtensionDescriptor extensionDescriptor) {
            PackageProperties properties = context.Package.PackageProperties;
            properties.Title = extensionDescriptor.DisplayName ?? extensionDescriptor.Name;
            //properties.Subject = "";
            properties.Creator = extensionDescriptor.Author;
            properties.Keywords = extensionDescriptor.Tags;
            properties.Description = extensionDescriptor.Description;
            //properties.LastModifiedBy = "";
            //properties.Revision = "";
            //properties.LastPrinted = "";
            //properties.Created = "";
            //properties.Modified = "";
            properties.Category = extensionDescriptor.Features.Where(f => f.Name == extensionDescriptor.Name).Select(f => f.Category).FirstOrDefault();
            properties.Identifier = extensionDescriptor.Name;
            properties.ContentType = "Orchard " + extensionDescriptor.ExtensionType;
            //properties.Language = "";
            properties.Version = extensionDescriptor.Version;
            properties.ContentStatus = "";
        }


        private void EmbedProjectFiles(CreateContext context, params string[] itemGroupTypes) {
            IEnumerable<XElement> itemGroups = context.Project
                .Elements(Ns("Project"))
                .Elements(Ns("ItemGroup"));

            foreach (string itemGroupType in itemGroupTypes) {
                IEnumerable<string> includePaths = itemGroups
                    .Elements(Ns(itemGroupType))
                    .Attributes("Include")
                    .Select(x => x.Value);
                foreach (string includePath in includePaths) {
                    EmbedVirtualFile(context, includePath, MediaTypeNames.Application.Octet);
                }
            }
        }

        private void EmbedReferenceFiles(CreateContext context) {
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
                if (context.SourceFolder.FileExists(context.SourcePath + virtualPath)) {
                    EmbedVirtualFile(context, virtualPath, MediaTypeNames.Application.Octet);
                }
                else if (hintPath != null) {}
            }
        }

        private void EmbedThemeFiles(CreateContext context) {
            var basePath = context.SourcePath;
            foreach (var virtualPath in context.SourceFolder.ListFiles(context.SourcePath, true)) {
                // ignore dlls, etc
                if (IgnoreFile(virtualPath)) {
                    continue;
                }
                // full virtual path given but we need the relative path so it can be put into
                // the package that way (the package itself is the logical base path).
                // Get it by stripping the basePath off including the slash.
                var relativePath = virtualPath.Replace(basePath, "");
                EmbedVirtualFile(context, relativePath, MediaTypeNames.Application.Octet);
            }
        }

        private XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }


        private static void BeginPackage(CreateContext context) {
            context.Stream = new MemoryStream();
            context.Package = Package.Open(context.Stream, FileMode.Create, FileAccess.ReadWrite);
        }

        private static void EstablishPaths(CreateContext context, IWebSiteFolder webSiteFolder, string locationPath, string moduleName, string moduleType) {
            context.SourceFolder = webSiteFolder;
            if (moduleType == "Theme") {
                context.SourcePath = "~/Themes/" + moduleName + "/";
            }
            else {
                context.SourcePath = "~/Modules/" + moduleName + "/";
            }
            context.TargetPath = "\\" + moduleName + "\\";
        }

        private static bool LoadProject(CreateContext context, string relativePath) {
            string virtualPath = context.SourcePath + relativePath;
            if (context.SourceFolder.FileExists(virtualPath)) {
                context.Project = XDocument.Parse(context.SourceFolder.ReadFile(context.SourcePath + relativePath));
                return true;
            }
            return false;
        }

        private static Uri EmbedVirtualFile(CreateContext context, string relativePath, string contentType) {
            Uri partUri = PackUriHelper.CreatePartUri(new Uri(context.TargetPath + relativePath, UriKind.Relative));
            PackagePart packagePart = context.Package.CreatePart(partUri, contentType);
            using (Stream stream = packagePart.GetStream(FileMode.Create, FileAccess.Write)) {
                context.SourceFolder.CopyFileTo(context.SourcePath + relativePath, stream, true /*actualContent*/);
            }
            return partUri;
        }

        private static void EndPackage(CreateContext context) {
            context.Package.Close();
        }

        #region Nested type: CreateContext

        private class CreateContext {
            public Stream Stream { get; set; }
            public Package Package { get; set; }

            public IWebSiteFolder SourceFolder { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }

            public XDocument Project { get; set; }
        }

        #endregion
    }
}