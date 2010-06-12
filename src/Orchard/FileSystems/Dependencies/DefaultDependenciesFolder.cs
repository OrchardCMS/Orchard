using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;

namespace Orchard.FileSystems.Dependencies {
    public class DefaultDependenciesFolder : IDependenciesFolder {
        private readonly string _basePath = "Dependencies";
        private readonly string _persistanceFileName = "dependencies.xml";
        private readonly ICacheManager _cacheManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultDependenciesFolder(ICacheManager cacheManager, IAppDataFolder appDataFolder) {
            _cacheManager = cacheManager;
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();
        }

        private string PersistencePath {
            get {
                return _appDataFolder.Combine(_basePath, _persistanceFileName);
            }
        }

        private IEnumerable<DependencyDescriptor> Descriptors {
            get {
                return _cacheManager.Get(PersistencePath,
                                         ctx => {
                                             _appDataFolder.CreateDirectory(_basePath);

                                             ctx.Monitor(_appDataFolder.WhenPathChanges(ctx.Key));
                                             ctx.Monitor(_writeThroughToken);

                                             _appDataFolder.CreateDirectory(_basePath);
                                             return ReadDependencies(ctx.Key).ToList();
                                         });
            }
        }

        public class InvalidationToken : IVolatileToken {
            public bool IsCurrent { get; set; }
        }

        public void StorePrecompiledAssembly(string moduleModuleName, string virtualPath, string loaderName) {
            _appDataFolder.CreateDirectory(_basePath);

            // Only store assembly if it's more recent that what we have stored already (if anything)
            var assemblyFileName = _appDataFolder.MapPath(virtualPath);
            if (IsNewerAssembly(moduleModuleName, assemblyFileName)) {
                var destinationFileName = Path.GetFileName(assemblyFileName);
                var destinationPath = _appDataFolder.MapPath(_appDataFolder.Combine(_basePath, destinationFileName));
                File.Copy(assemblyFileName, destinationPath, true);

                StoreDepencyInformation(new DependencyDescriptor {
                    ModuleName = moduleModuleName,
                    LoaderName = loaderName,
                    VirtualPath = virtualPath,
                    FileName = destinationFileName
                });
            }
        }

        public void Remove(string moduleName, string loaderName) {
            Func<DependencyDescriptor, bool> predicate = (d => d.ModuleName == moduleName && d.LoaderName == loaderName);
            if (Descriptors.Any(predicate)) {
                var newDescriptors = Descriptors.Where(e => !predicate(e));

                WriteDependencies(PersistencePath, newDescriptors);
            }
        }

        public void Store(DependencyDescriptor descriptor) {
            StoreDepencyInformation(descriptor);
        }

        public DependencyDescriptor GetDescriptor(string moduleName) {
            return Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
        }

        private bool IsNewerAssembly(string moduleName, string assemblyFileName) {
            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
            if (dependency == null) {
                return true;
            }

            var existingFileName = _appDataFolder.MapPath(_appDataFolder.Combine(_basePath, dependency.FileName));
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

        public Assembly LoadAssembly(string moduleName) {
            _appDataFolder.CreateDirectory(_basePath);

            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
            if (dependency == null)
                return null;

            if (!_appDataFolder.FileExists(_appDataFolder.Combine(_basePath, dependency.FileName)))
                return null;

            return Assembly.Load(Path.GetFileNameWithoutExtension(dependency.FileName));
        }

        public bool HasPrecompiledAssembly(string moduleName) {
            var dependency = Descriptors.SingleOrDefault(d => d.ModuleName == moduleName);
            if (dependency == null)
                return false;

            if (!_appDataFolder.FileExists(_appDataFolder.Combine(_basePath, dependency.FileName)))
                return false;

            return true;
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
                        LoaderName = elem(e, "LoaderName")
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
                                                                 new XElement(ns("LoaderName"), d.LoaderName),
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