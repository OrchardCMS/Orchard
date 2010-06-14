using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.Localization;

namespace Orchard.FileSystems.Dependencies {
    public class DefaultDependenciesFolder : IDependenciesFolder {
        private const string BasePath = "Dependencies";
        private const string FileName = "dependencies.xml";
        private readonly ICacheManager _cacheManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultDependenciesFolder(ICacheManager cacheManager, IAppDataFolder appDataFolder) {
            _cacheManager = cacheManager;
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private string PersistencePath {
            get {
                return _appDataFolder.Combine(BasePath, FileName);
            }
        }

        private IEnumerable<DependencyDescriptor> Descriptors {
            get {
                return _cacheManager.Get(PersistencePath,
                                         ctx => {
                                             _appDataFolder.CreateDirectory(BasePath);

                                             ctx.Monitor(_appDataFolder.WhenPathChanges(ctx.Key));
                                             ctx.Monitor(_writeThroughToken);

                                             _appDataFolder.CreateDirectory(BasePath);
                                             return ReadDependencies(ctx.Key).ToList();
                                         });
            }
        }

        public IEnumerable<DependencyDescriptor> LoadDescriptors() {
            return Descriptors;
        }

        public void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors) {
            WriteDependencies(PersistencePath, dependencyDescriptors);
        }

        public DependencyDescriptor GetDescriptor(string moduleName) {
            return Descriptors.SingleOrDefault(d => d.Name == moduleName);
        }

        public ProbingAssembly GetProbingAssembly(string moduleName) {
            var path = PrecompiledAssemblyPath(moduleName);
            if (!_appDataFolder.FileExists(path))
                return null;

            return new ProbingAssembly {
                Path = path,
                Assembly = () => Assembly.Load(moduleName),
                LastWriteTimeUtc = () => File.GetLastWriteTimeUtc(_appDataFolder.MapPath(path)),
            };
        }

        public string GetProbingAssemblyPhysicalFileName(string moduleName) {
            return _appDataFolder.MapPath(PrecompiledAssemblyPath(moduleName));
        }

        private string PrecompiledAssemblyPath(string moduleName) {
            return _appDataFolder.Combine(BasePath, moduleName + ".dll");
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
                        Name = elem(e, "ModuleName"),
                        VirtualPath = elem(e, "VirtualPath"),
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
                                                                 new XElement(ns("ModuleName"), d.Name),
                                                                 new XElement(ns("VirtualPath"), d.VirtualPath),
                                                                 new XElement(ns("LoaderName"), d.LoaderName)));
            document.Root.Add(elements);

            using (var stream = _appDataFolder.CreateFile(persistancePath)) {
                document.Save(stream, SaveOptions.None);
            }

            // Ensure cache is invalidated right away, not waiting for file change notification to happen
            _writeThroughToken.IsCurrent = false;
        }

        private class InvalidationToken : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
    }
}