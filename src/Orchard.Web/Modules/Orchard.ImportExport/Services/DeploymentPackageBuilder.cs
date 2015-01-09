using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NuGet;
using Orchard.ContentManagement.Handlers;
using Orchard.FileSystems.WebSite;

namespace Orchard.ImportExport.Services {
    public interface IDeploymentPackageBuilder : IDependency {
        Stream BuildPackage(string packageName, XDocument recipe, IList<ExportedFileDescription> files);
    }

    public class DeploymentPackageBuilder : IDeploymentPackageBuilder {
        private readonly IWebSiteFolder _webSiteFolder;

        public DeploymentPackageBuilder(
            IWebSiteFolder webSiteFolder
            ) {
            _webSiteFolder = webSiteFolder;
        }

        public Stream BuildPackage(string packageName, XDocument recipe, IList<ExportedFileDescription> files) {
            var context = new CreateContext {
                SourcePath = "~/",
                TargetPath = @"\Content\",
                SourceFolder = _webSiteFolder,
                Files = files
            };
            BeginPackage(context);
            try {
                SetCoreProperties(context, packageName);

                //EmbedVirtualFile(context, recipePath);
                EmbedFiles(context);
            }
            finally {
                EndPackage(context);
            }

            if (context.Stream.CanSeek) {
                context.Stream.Seek(0, SeekOrigin.Begin);
            }

            return context.Stream;
        }

        private static void SetCoreProperties(CreateContext context, string packageName) {
            context.Builder.Id = packageName;
            context.Builder.Version = new Version();
            context.Builder.Title = packageName;
            context.Builder.Description = "";
        }
        
        private void EmbedFiles(CreateContext context) {
            foreach (var file in context.Files) {
                EmbedVirtualFile(context, file.LocalPath);
            }
        }

        private static void BeginPackage(CreateContext context) {
            context.Stream = new MemoryStream();
            context.Builder = new PackageBuilder();
        }

        private static void EmbedVirtualFile(CreateContext context, string relativePath) {
            var file = new VirtualPackageFile(
                context.SourceFolder,
                context.SourcePath + relativePath, 
                context.TargetPath + relativePath);
            context.Builder.Files.Add(file);
        }

        private static void EndPackage(CreateContext context) {
            context.Builder.Save(context.Stream);
        }

        private class CreateContext {
            public Stream Stream { get; set; }
            public PackageBuilder Builder { get; set; }

            public IWebSiteFolder SourceFolder { get; set; }
            public string SourcePath { get; set; }
            public string TargetPath { get; set; }

            public IList<ExportedFileDescription> Files { get; set; }
        }

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
    }
}