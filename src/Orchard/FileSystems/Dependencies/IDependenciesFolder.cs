using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Environment.Topology;

namespace Orchard.FileSystems.Dependencies {
    public class DependencyDescriptor {
        public string ModuleName { get; set; }
        public bool IsFromBuildProvider { get; set; }
        public string VirtualPath { get; set; }
        public string FileName { get; set; }
    }

    public interface IDependenciesFolder : IVolatileProvider {
        void StoreBuildProviderAssembly(string moduleName, string virtualPath, Assembly assembly);
        void StorePrecompiledAssembly(string moduleName, string virtualPath);
        DependencyDescriptor GetDescriptor(string moduleName);
        Assembly LoadAssembly(string assemblyName);
    }

    public class DefaultDependenciesFolder : IDependenciesFolder {
        private readonly string _prefix = Guid.NewGuid().ToString("n");
        private const string _basePath = "~/App_Data/Dependencies";
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IExtensionManagerEvents _events;

        public DefaultDependenciesFolder(IVirtualPathProvider virtualPathProvider, IExtensionManagerEvents events) {
            _virtualPathProvider = virtualPathProvider;
            _events = events;
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

        public void StoreBuildProviderAssembly(string moduleName, string virtualPath, Assembly assembly) {
            _virtualPathProvider.CreateDirectory(BasePath);

            var descriptor = new DependencyDescriptor {
                ModuleName = moduleName,
                IsFromBuildProvider = true,
                VirtualPath = virtualPath,
                FileName = assembly.Location
            };

            StoreDepencyInformation(descriptor);

#if true
            var cacheDependency = HostingEnvironment.VirtualPathProvider.GetCacheDependency(
                virtualPath,
                new[] { virtualPath },
                DateTime.UtcNow);

            HostingEnvironment.Cache.Add(
                _prefix + virtualPath,
                moduleName,
                cacheDependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                (key, value, reason) => _events.ModuleChanged((string) value));
#endif
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
            return ReadDependencies().SingleOrDefault(d => d.ModuleName == moduleName);
        }

        private bool IsNewerAssembly(string moduleName, string assemblyFileName) {
            var dependency = ReadDependencies().SingleOrDefault(d => d.ModuleName == moduleName);
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
            var dependencies = ReadDependencies().ToList();
            int index = dependencies.FindIndex(d => d.ModuleName == descriptor.ModuleName);
            if (index < 0) {
                dependencies.Add(descriptor);
            }
            else {
                dependencies[index] = descriptor;
            }

            WriteDependencies(dependencies);
        }

        public Assembly LoadAssembly(string assemblyName) {
            _virtualPathProvider.CreateDirectory(BasePath);

            var dependency = ReadDependencies().SingleOrDefault(d => d.ModuleName == assemblyName);
            if (dependency == null)
                return null;

            if (!_virtualPathProvider.FileExists(_virtualPathProvider.Combine(BasePath, dependency.FileName)))
                return null;

            return Assembly.Load(Path.GetFileNameWithoutExtension(dependency.FileName));
        }

        private IEnumerable<DependencyDescriptor> ReadDependencies() {
            if (!_virtualPathProvider.FileExists(PersistencePath))
                return Enumerable.Empty<DependencyDescriptor>();

            using (var stream = _virtualPathProvider.OpenFile(PersistencePath)) {
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

        private void WriteDependencies(IEnumerable<DependencyDescriptor> dependencies) {
            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = dependencies.Select(d => new XElement("Dependency",
                                                  new XElement(ns("ModuleName"), d.ModuleName),
                                                  new XElement(ns("VirtualPath"), d.VirtualPath),
                                                  new XElement(ns("IsFromBuildProvider"), d.IsFromBuildProvider),
                                                  new XElement(ns("FileName"), d.FileName)));
            document.Root.Add(elements);

            using (var stream = _virtualPathProvider.CreateText(PersistencePath)) {
                document.Save(stream, SaveOptions.None);
            }
        }

        private static XName ns(string name) {
            return XName.Get(name/*, "http://schemas.microsoft.com/developer/msbuild/2003"*/);
        }
    }
}
