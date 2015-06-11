using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Xml.Linq;
using NuGet;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.FileSystems.WebSite;
using Orchard.Utility.Extensions;
using NuGetPackageBuilder = NuGet.PackageBuilder;

namespace Orchard.Packaging.Services {
    [OrchardFeature("PackagingServices")]
    public class PackageBuilder : IPackageBuilder {
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IOrchardFrameworkAssemblies _frameworkAssemblies;

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

        public PackageBuilder(IWebSiteFolder webSiteFolder,
            IVirtualPathProvider virtualPathProvider,
            IOrchardFrameworkAssemblies frameworkAssemblies) {

            _webSiteFolder = webSiteFolder;
            _virtualPathProvider = virtualPathProvider;
            _frameworkAssemblies = frameworkAssemblies;
        }

        public Stream BuildPackage(ExtensionDescriptor extensionDescriptor) {
            var context = new CreateContext();
            BeginPackage(context);
            try {
                EstablishPaths(context, _webSiteFolder, extensionDescriptor.Location, extensionDescriptor.Id, extensionDescriptor.ExtensionType);
                SetCoreProperties(context, extensionDescriptor);

                string projectFile = extensionDescriptor.Id + ".csproj";
                if (LoadProject(context, projectFile)) {
                    EmbedVirtualFile(context, projectFile, MediaTypeNames.Text.Xml);
                    EmbedProjectFiles(context, "Compile", "Content", "None", "EmbeddedResource");
                    EmbedReferenceFiles(context);
                } else if (DefaultExtensionTypes.IsTheme(extensionDescriptor.ExtensionType)) {
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

        public static string BuildPackageId(string extensionName, string extensionType) {
            return PackagingSourceManager.GetExtensionPrefix(extensionType) + extensionName;
        }

        private static void SetCoreProperties(CreateContext context, ExtensionDescriptor extensionDescriptor) {
            context.Builder.Id = BuildPackageId(extensionDescriptor.Id, extensionDescriptor.ExtensionType);
            context.Builder.Version = new Version(extensionDescriptor.Version);
            context.Builder.Title = extensionDescriptor.Name ?? extensionDescriptor.Id;
            context.Builder.Description = extensionDescriptor.Description;
            context.Builder.Authors.Add(extensionDescriptor.Author);

            if(Uri.IsWellFormedUriString(extensionDescriptor.WebSite, UriKind.Absolute)) {
                context.Builder.ProjectUrl = new Uri(extensionDescriptor.WebSite);
            }
        }
        
        private static void EmbedProjectFiles(CreateContext context, params string[] itemGroupTypes) {
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

                // If it is not a core assembly
                if (_frameworkAssemblies.GetFrameworkAssemblies().FirstOrDefault(assembly => assembly.Name.Equals(assemblyName.Name)) == null) {
                    string virtualPath = _virtualPathProvider.GetReferenceVirtualPath(context.SourcePath, assemblyName.Name, entry.HintPath != null ? entry.HintPath.Value : null);

                    if (!string.IsNullOrEmpty(virtualPath)) {
                        EmbedVirtualFile(context, virtualPath, MediaTypeNames.Application.Octet);
                    }
                }
            }
        }

        private static void EmbedThemeFiles(CreateContext context) {
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

        private static XName Ns(string localName) {
            return XName.Get(localName, "http://schemas.microsoft.com/developer/msbuild/2003");
        }


        private static void BeginPackage(CreateContext context) {
            context.Stream = new MemoryStream();
            context.Builder = new NuGetPackageBuilder();
        }

        private static void EstablishPaths(CreateContext context, IWebSiteFolder webSiteFolder, string locationPath, string moduleName, string moduleType) {
            context.SourceFolder = webSiteFolder;
            if (DefaultExtensionTypes.IsTheme(moduleType)) {
                context.SourcePath = "~/Themes/" + moduleName + "/";
                context.TargetPath = "\\Content\\Themes\\" + moduleName + "\\";
            }
            else {
                context.SourcePath = "~/Modules/" + moduleName + "/";
                context.TargetPath = "\\Content\\Modules\\" + moduleName + "\\";
            }
        }

        private static bool LoadProject(CreateContext context, string relativePath) {
            string virtualPath = context.SourcePath + relativePath;
            if (context.SourceFolder.FileExists(virtualPath)) {
                context.Project = XDocument.Parse(context.SourceFolder.ReadFile(context.SourcePath + relativePath));
                return true;
            }
            return false;
        }

        private static void EmbedVirtualFile(CreateContext context, string relativePath, string contentType) {
            var file = new VirtualPackageFile(
                context.SourceFolder,
                context.SourcePath + relativePath, 
                context.TargetPath + relativePath);
            context.Builder.Files.Add(file);
        }

        private static void EndPackage(CreateContext context) {
            context.Builder.Save(context.Stream);
        }

        #region Nested type: CreateContext

        private class CreateContext {
            public Stream Stream { get; set; }
            public NuGetPackageBuilder Builder { get; set; }

            public IWebSiteFolder SourceFolder { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }

            public XDocument Project { get; set; }
        }

        #endregion

        #region Nested type: CreateContext

        private class VirtualPackageFile : IPackageFile {
            private readonly IWebSiteFolder _webSiteFolder;
            private readonly string _virtualPath;
            private readonly string _packagePath;

            public VirtualPackageFile(IWebSiteFolder webSiteFolder, string virtualPath, string packagePath) {
                _webSiteFolder = webSiteFolder;
                _virtualPath = virtualPath;
                _packagePath = packagePath;
            }

            public string Path { get { return _packagePath; } }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Supposed to return an open stream.")]
            public Stream GetStream() {
                var stream = new MemoryStream();
                _webSiteFolder.CopyFileTo(_virtualPath, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }

        #endregion
    }
}