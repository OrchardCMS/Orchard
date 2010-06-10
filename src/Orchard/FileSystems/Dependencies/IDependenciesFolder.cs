using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.WebSite;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public string ModuleName { get; set; }
        public bool IsFromBuildProvider { get; set; }
        public string VirtualPath { get; set; }
        public string FileName { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        void StoreReferencedAssembly(string moduleName);
        void StorePrecompiledAssembly(string moduleName, string virtualPath);
        void StoreBuildProviderAssembly(string moduleName, string virtualPath, Assembly assembly);
        DependencyDescriptor GetDescriptor(string moduleName);
        Assembly LoadAssembly(string assemblyName);
    }

    public class DefaultDependenciesFolder : IDependenciesFolder {
        private readonly string _basePath = "~/App_Data/Dependencies";
        private readonly ICacheManager _cacheManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IExtensionManagerEvents _events;
        private readonly InvalidationToken _token;

        public DefaultDependenciesFolder(ICacheManager cacheManager, IWebSiteFolder webSiteFolder, IVirtualPathProvider virtualPathProvider, IExtensionManagerEvents events) {
            _cacheManager = cacheManager;
            _webSiteFolder = webSiteFolder;
            _virtualPathProvider = virtualPathProvider;
            _events = events;
            _token = new InvalidationToken();
        }

        private string BasePath {
            get {
                return _basePath;
            }
        }

        private string PersistencePath {
            get {
                return _virtualPathProvider.Combine(BasePath, "dependencies.xml");
            }
        }

        private IList<DependencyDescriptor> Descriptors {
            get {
                return _cacheManager.Get(PersistencePath,
                    ctx => {
                        ctx.Monitor(_webSiteFolder.WhenPathChanges(ctx.Key));
                        ctx.Monitor(_token);

                        _virtualPathProvider.CreateDirectory(BasePath);
                        return ReadDependencies(ctx.Key);
                    });
            }
        }

        public class InvalidationToken : IVolatileToken {
            public bool IsCurrent { get; set; }
        }

        public void StoreReferencedAssembly(string moduleName) {
            if (Descriptors.Any(d => d.ModuleName == moduleName)) {
                // Remove the moduleName from the list of assemblies in the dependency folder
                var newDescriptors = Descriptors.Where(d => d.ModuleName != moduleName);

                WriteDependencies(PersistencePath, newDescriptors);
            }
        }

        public void StoreBuildProviderAssembly(string moduleName, string virtualPath, Assembly assembly) {
            var descriptor = new DependencyDescriptor {
                ModuleName = moduleName,
                IsFromBuildProvider = true,
                VirtualPath = virtualPath,
                FileName = assembly.Location
            };

            StoreDepencyInformation(descriptor);

            _webSiteFolder.WhenPathChanges(virtualPath, () => _events.ModuleChanged(moduleName));
        }

        public void StorePrecompiledAssembly(string moduleName, string virtualPath) {
            _virtualPathProvider.CreateDirectory(BasePath);

            // Only store assembly if it's more recent that what we have stored already (if anything)
            var assemblyFileName = _virtualPathProvider.MapPath(virtualPath);
            if (IsNewerAssembly(moduleName, assemblyFileName)) {
                var destinationFileName = Path.GetFileName(assemblyFileName);
                var destinationPath = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(BasePath, destinationFileName));
                File.Copy(assemblyFileName, destinationPath, true);

                StoreDepencyInformation(new DependencyDescriptor {
                    ModuleName = moduleName,
                    IsFromBuildProvider = false,
                    VirtualPath = virtualPath,
                    FileName = destinationFileName
                });
            }
        }

        public DependencyDescriptor GetDescriptor(string moduleName) {
            return Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
        }

        private bool IsNewerAssembly(string moduleName, string assemblyFileName) {
            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
            if (dependency == null) {
                return true;
            }

            var existingFileName = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(BasePath, dependency.FileName));
            if (!File.Exists(existingFileName)) {
                return true;
            }

            return (File.GetLastWriteTimeUtc(existingFileName) < File.GetLastWriteTimeUtc(assemblyFileName));
        }

        private void StoreDepencyInformation(DependencyDescriptor descriptor) {
            var dependencies = Descriptors.ToList();
            int index = dependencies.FindIndex(d => d.ModuleName == descriptor.ModuleName);
            if (index < 0) {
                dependencies.Add(descriptor);
            }
            else {
                dependencies[index] = descriptor;
            }

            WriteDependencies(PersistencePath, dependencies);
        }

        public Assembly LoadAssembly(string assemblyName) {
            _virtualPathProvider.CreateDirectory(BasePath);

            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == assemblyName);
            if (dependency == null)
                return null;

            if (!_virtualPathProvider.FileExists(_virtualPathProvider.Combine(BasePath, dependency.FileName)))
                return null;

            return Assembly.Load(Path.GetFileNameWithoutExtension(dependency.FileName));
        }

        private IEnumerable<DependencyDescriptor> ReadDependencies(string persistancePath) {
            Func<string, XName> ns = (name => XName.Get(name));

            if (!_virtualPathProvider.FileExists(persistancePath))
                return Enumerable.Empty<DependencyDescriptor>();

            using (var stream = _virtualPathProvider.OpenFile(persistancePath)) {
                XDocument document = XDocument.Load(stream);
                return document
                    .Elements(ns("Dependencies"))
                    .Elements(ns("Dependency"))
                    .Select(e => new DependencyDescriptor {
                        ModuleName = e.Element("ModuleName").Value,
                        VirtualPath = e.Element("VirtualPath").Value,
                        FileName = e.Element("FileName").Value,
                        IsFromBuildProvider = bool.Parse(e.Element("IsFromBuildProvider").Value)
                    })
                    .ToList();
            }
        }

        private void WriteDependencies(string persistancePath, IEnumerable<DependencyDescriptor> dependencies) {
            Func<string, XName> ns = (name => XName.Get(name));

            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = dependencies.Select(d => new XElement("Dependency",
                                                  new XElement(ns("ModuleName"), d.ModuleName),
                                                  new XElement(ns("VirtualPath"), d.VirtualPath),
                                                  new XElement(ns("IsFromBuildProvider"), d.IsFromBuildProvider),
                                                  new XElement(ns("FileName"), d.FileName)));
            document.Root.Add(elements);

            using (var stream = _virtualPathProvider.CreateText(persistancePath)) {
                document.Save(stream, SaveOptions.None);
            }

            // Ensure cache is invalidated right away, not waiting for file change notification to happen
            _token.IsCurrent = false;
        }
    }
}
