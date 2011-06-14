using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.Localization;

namespace Orchard.Environment.Compilation.Dependencies {
    /// <summary>
    /// Provides an abstraction used to manage the dependency descriptors in the dependencies folder.
    /// The dependecy descriptors provide information about the different extensions, and loaders used to load them.
    /// By default they are stored in the dependencies.xml file in the dependencies folder.
    /// </summary>
    public class DefaultDependencyDescriptorManager : IDependencyDescriptorManager {
        private const string BasePath = "Dependencies";
        private const string FileName = "dependencies.xml";
        private readonly ICacheManager _cacheManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDependencyDescriptorManager"/> class.
        /// </summary>
        /// <param name="cacheManager">The cache manager.</param>
        /// <param name="appDataFolder">The app data folder.</param>
        public DefaultDependencyDescriptorManager(ICacheManager cacheManager, IAppDataFolder appDataFolder) {
            _cacheManager = cacheManager;
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();
            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// Gets or sets the localizer.
        /// </summary>
        public Localizer T { get; set; }

        private string PersistencePath {
            get { return _appDataFolder.Combine(BasePath, FileName); }
        }

        /// <summary>
        /// Retrieves the dependency descriptor for a module.
        /// </summary>
        /// <param name="moduleName">The module's name.</param>
        /// <returns>The dependency descriptor for the module.</returns>
        public DependencyDescriptor GetDescriptor(string moduleName) {
            return LoadDescriptors().SingleOrDefault(d => StringComparer.OrdinalIgnoreCase.Equals(d.Name, moduleName));
        }

        /// <summary>
        /// Loads the dependency descriptors from the dependencies folder.
        /// </summary>
        /// <returns>A collection of the dependency descriptors.</returns>
        public IEnumerable<DependencyDescriptor> LoadDescriptors() {
            return _cacheManager.Get(PersistencePath,
                                     ctx => {
                                         _appDataFolder.CreateDirectory(BasePath);
                                         ctx.Monitor(_appDataFolder.WhenPathChanges(ctx.Key));

                                         _writeThroughToken.IsCurrent = true;
                                         ctx.Monitor(_writeThroughToken);

                                         return ReadDependencies(ctx.Key).ToList();
                                     });
        }

        /// <summary>
        /// Stores a collection of dependencies descriptors into the dependencies folder.
        /// </summary>
        /// <param name="dependencyDescriptors">The collection to be stored.</param>
        public void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors) {
            var existingDescriptors = LoadDescriptors().OrderBy(d => d.Name);
            var newDescriptors = dependencyDescriptors.OrderBy(d => d.Name);

            if (!newDescriptors.SequenceEqual(existingDescriptors, new DependencyDescriptorComparer())) {
                WriteDependencies(PersistencePath, dependencyDescriptors);
            }
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
                        LoaderName = elem(e, "LoaderName"),
                        References = e.Elements(ns("References")).Elements(ns("Reference")).Select(r => new DependencyReferenceDescriptor {
                            Name = elem(r, "Name"),
                            LoaderName = elem(r, "LoaderName"),
                            VirtualPath = elem(r, "VirtualPath")
                    })}).ToList();
            }
        }

        private void WriteDependencies(string persistancePath, IEnumerable<DependencyDescriptor> dependencies) {
            Func<string, XName> ns = name => XName.Get(name);

            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = dependencies.Select(d => new XElement("Dependency",
                                                                 new XElement(ns("ModuleName"), d.Name),
                                                                 new XElement(ns("VirtualPath"), d.VirtualPath),
                                                                 new XElement(ns("LoaderName"), d.LoaderName),
                                                                 new XElement(ns("References"), d.References
                                                                     .Select(r => new XElement(ns("Reference"),
                                                                        new XElement(ns("Name"), r.Name),
                                                                        new XElement(ns("LoaderName"), r.LoaderName),
                                                                        new XElement(ns("VirtualPath"), r.VirtualPath))).ToArray())));

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

        private class DependencyDescriptorComparer : EqualityComparer<DependencyDescriptor> {
            private readonly ReferenceDescriptorComparer _referenceDescriptorComparer = new ReferenceDescriptorComparer();

            public override bool Equals(DependencyDescriptor x, DependencyDescriptor y) {
                return
                    StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.LoaderName, y.LoaderName) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.VirtualPath, y.VirtualPath) &&
                    x.References.SequenceEqual(y.References, _referenceDescriptorComparer);
            }

            public override int GetHashCode(DependencyDescriptor obj) {
                return
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.LoaderName) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VirtualPath) ^
                    obj.References.Aggregate(0, (a, entry) => a + _referenceDescriptorComparer.GetHashCode(entry));
            }
        }

        private class ReferenceDescriptorComparer : EqualityComparer<DependencyReferenceDescriptor> {
            public override bool Equals(DependencyReferenceDescriptor x, DependencyReferenceDescriptor y) {
                return
                    StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.LoaderName, y.LoaderName) &&
                    StringComparer.OrdinalIgnoreCase.Equals(x.VirtualPath, y.VirtualPath);
            }

            public override int GetHashCode(DependencyReferenceDescriptor obj) {
                return
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.LoaderName) ^
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VirtualPath);
            }
        }
    }
}