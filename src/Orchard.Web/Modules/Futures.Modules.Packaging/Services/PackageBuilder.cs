using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.WebSite;

namespace Futures.Modules.Packaging.Services {
    public class PackageBuilder : IPackageBuilder {
        private readonly IExtensionManager _extensionManager;
        private readonly IWebSiteFolder _webSiteFolder;

        public PackageBuilder(IExtensionManager extensionManager, IWebSiteFolder webSiteFolder) {
            _extensionManager = extensionManager;
            _webSiteFolder = webSiteFolder;
        }

        class CreateContext {
            public Stream Stream { get; set; }
            public Package Package { get; set; }

            public IWebSiteFolder SourceFolder { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }

            public XDocument Project { get; set; }
        }

        public Stream BuildPackage(ExtensionDescriptor extensionDescriptor) {
            var context = new CreateContext();
            BeginPackage(context);
            try {
                EstablishPaths(context, _webSiteFolder, extensionDescriptor.Location, extensionDescriptor.Name);
                SetCoreProperties(context, extensionDescriptor);

                var projectFile = extensionDescriptor.Name + ".csproj";
                if (LoadProject(context, projectFile)) {
                    EmbedVirtualFile(context, projectFile, System.Net.Mime.MediaTypeNames.Text.Xml);
                    EmbedProjectFiles(context, "Compile", "Content", "None", "EmbeddedResource");
                    EmbedReferenceFiles(context);
                }
            }
            finally {
                EndPackage(context);
            }

            return context.Stream;
        }

        private void SetCoreProperties(CreateContext context, ExtensionDescriptor extensionDescriptor) {
            var properties = context.Package.PackageProperties;
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
            var itemGroups = context.Project
                .Elements(Ns("Project"))
                .Elements(Ns("ItemGroup"));

            foreach (var itemGroupType in itemGroupTypes) {
                var includePaths = itemGroups
                    .Elements(Ns(itemGroupType))
                    .Attributes("Include")
                    .Select(x => x.Value);
                foreach (var includePath in includePaths) {
                    EmbedVirtualFile(context, includePath, System.Net.Mime.MediaTypeNames.Application.Octet);
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
                var hintPath = entry.HintPath != null ? entry.HintPath.Value : null;

                var virtualPath = "bin/" + assemblyName.Name + ".dll";
                if (context.SourceFolder.FileExists(context.SourcePath + virtualPath)) {
                    EmbedVirtualFile(context, virtualPath, System.Net.Mime.MediaTypeNames.Application.Octet);
                }
                else if (hintPath != null) {
                }
            }
        }

        private XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }


        static void BeginPackage(CreateContext context) {
            context.Stream = new MemoryStream();
            context.Package = Package.Open(context.Stream, FileMode.Create, FileAccess.ReadWrite);
        }

        static void EstablishPaths(CreateContext context, IWebSiteFolder webSiteFolder, string locationPath, string moduleName) {
            context.SourceFolder = webSiteFolder;
            context.SourcePath = "~/Modules/" + moduleName + "/";
            context.TargetPath = "\\" + moduleName + "\\";
        }

        static bool LoadProject(CreateContext context, string relativePath) {
            var virtualPath = context.SourcePath + relativePath;
            if (context.SourceFolder.FileExists(virtualPath)) {
                context.Project = XDocument.Parse(context.SourceFolder.ReadFile(context.SourcePath + relativePath));
                return true;
            }
            return false;
        }

        static Uri EmbedVirtualFile(CreateContext context, string relativePath, string contentType) {
            var partUri = PackUriHelper.CreatePartUri(new Uri(context.TargetPath + relativePath, UriKind.Relative));
            var packagePart = context.Package.CreatePart(partUri, contentType);
            using (var stream = packagePart.GetStream(FileMode.Create, FileAccess.Write)) {
                context.SourceFolder.CopyFileTo(context.SourcePath + relativePath, stream);
            }
            return partUri;
        }

        static void EndPackage(CreateContext context) {
            context.Package.Close();
        }
    }
}
