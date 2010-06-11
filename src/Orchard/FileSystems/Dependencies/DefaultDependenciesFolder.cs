using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.WebSite;

namespace Orchard.FileSystems.Dependencies {
    public class DefaultDependenciesFolder : IDependenciesFolder {
        private readonly string _basePath = "Dependencies";
        private readonly ICacheManager _cacheManager;
        private readonly IWebSiteFolder _webSiteFolder;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IExtensionManagerEvents _events;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultDependenciesFolder(ICacheManager cacheManager, IWebSiteFolder webSiteFolder, IAppDataFolder appDataFolder, IExtensionManagerEvents events) {
            _cacheManager = cacheManager;
            _webSiteFolder = webSiteFolder;
            _appDataFolder = appDataFolder;
            _events = events;
            _writeThroughToken = new InvalidationToken();
        }

        private string BasePath {
            get {
                return _basePath;
            }
        }

        private string PersistencePath {
            get {
                return _appDataFolder.Combine(BasePath, "dependencies.xml");
            }
        }

        private IEnumerable<DependencyDescriptor> Descriptors {
            get {
                return _cacheManager.Get(PersistencePath,
                                         ctx => {
                                             ctx.Monitor(_appDataFolder.WhenPathChanges(ctx.Key));
                                             ctx.Monitor(_writeThroughToken);

                                             _appDataFolder.CreateDirectory(BasePath);
                                             return ReadDependencies(ctx.Key).ToList();
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
            _appDataFolder.CreateDirectory(BasePath);

            // Only store assembly if it's more recent that what we have stored already (if anything)
            var assemblyFileName = _appDataFolder.MapPath(virtualPath);
            if (IsNewerAssembly(moduleName, assemblyFileName)) {
                var destinationFileName = Path.GetFileName(assemblyFileName);
                var destinationPath = _appDataFolder.MapPath(_appDataFolder.Combine(BasePath, destinationFileName));
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

            var existingFileName = _appDataFolder.MapPath(_appDataFolder.Combine(BasePath, dependency.FileName));
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
            _appDataFolder.CreateDirectory(BasePath);

            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == assemblyName);
            if (dependency == null)
                return null;

            if (!_appDataFolder.FileExists(_appDataFolder.Combine(BasePath, dependency.FileName)))
                return null;

            return Assembly.Load(Path.GetFileNameWithoutExtension(dependency.FileName));
        }

        private IEnumerable<DependencyDescriptor> ReadDependencies(string persistancePath) {
            Func<string, XName> ns = (name => XName.Get(name));
            Func<XElement, string, string> elem = (e, name) => e.Element(ns(name)).Value;

            if (!_appDataFolder.FileExists(persistancePath))
                return Enumerable.Empty<DependencyDescriptor>();

            using (var stream = _appDataFolder.OpenFile(persistancePath)) {
                XDocument document = XDocument.Load(stream);
                return document
                    .Elements(ns("Dependencies"))
                    .Elements(ns("Dependency"))
                    .Select(e => new DependencyDescriptor {
                        ModuleName = elem(e, "ModuleName"),
                        VirtualPath = elem(e, "VirtualPath"),
                        FileName = elem(e, "FileName"),
                        IsFromBuildProvider = bool.Parse(elem(e, "IsFromBuildProvider"))
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

            using (var stream = _appDataFolder.CreateFile(persistancePath)) {
                document.Save(stream, SaveOptions.None);
            }

            // Ensure cache is invalidated right away, not waiting for file change notification to happen
            _writeThroughToken.IsCurrent = false;
        }
    }
}