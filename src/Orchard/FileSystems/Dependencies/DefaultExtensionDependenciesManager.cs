using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    /// <summary>
    /// Similar to "Dependencies.xml" file, except we also store "GetFileHash" result for every 
    /// VirtualPath entry. This is so that if any virtual path reference in the file changes,
    /// the file stored by this component will also change.
    /// </summary>
    public class DefaultExtensionDependenciesManager : IExtensionDependenciesManager {
        private const string BasePath = "Dependencies";
        private const string FileName = "Dependencies.ModuleCompilation.xml";
        private readonly ICacheManager _cacheManager;
        private readonly IAppDataFolder _appDataFolder;
        private readonly InvalidationToken _writeThroughToken;

        public DefaultExtensionDependenciesManager(ICacheManager cacheManager, IAppDataFolder appDataFolder) {
            _cacheManager = cacheManager;
            _appDataFolder = appDataFolder;
            _writeThroughToken = new InvalidationToken();

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private string PersistencePath {
            get { return _appDataFolder.Combine(BasePath, FileName); }
        }

        public void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors, Func<string, string> fileHashProvider) {
            Logger.Information("Storing module dependency file.");

            var newDocument = CreateDocument(dependencyDescriptors, fileHashProvider);
            var previousDocument = ReadDocument(PersistencePath);
            if (CompareXmlDocuments(newDocument, previousDocument)) {
                Logger.Debug("Existing document is identical to new one. Skipping save.");
            }
            else {
                WriteDocument(PersistencePath, newDocument);
            }

            Logger.Information("Done storing module dependency file.");
        }

        public IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor descriptor) {
            if (IsSupportedLoader(descriptor.LoaderName)) {
                // Currently, we return the same file for every module. An improvement would be to return
                // a specific file per module (this would decrease the number of recompilations needed
                // when modules change on disk).
                yield return _appDataFolder.GetVirtualPath(PersistencePath);
            }
        }

        public ActivatedExtensionDescriptor GetDescriptor(string extensionId) {
            return LoadDescriptors().FirstOrDefault(d => d.ExtensionId.Equals(extensionId, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ActivatedExtensionDescriptor> LoadDescriptors() {
            return _cacheManager.Get(PersistencePath,
                                     ctx => {
                                         _appDataFolder.CreateDirectory(BasePath);
                                         ctx.Monitor(_appDataFolder.WhenPathChanges(ctx.Key));

                                         _writeThroughToken.IsCurrent = true;
                                         ctx.Monitor(_writeThroughToken);

                                         return ReadDescriptors(ctx.Key).ToList();
                                     });
        }

        private XDocument CreateDocument(IEnumerable<DependencyDescriptor> dependencies, Func<string, string> fileHashProvider) {
            Func<string, XName> ns = (name => XName.Get(name));

            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = FilterDependencies(dependencies).Select(
                d => new XElement("Dependency",
                    new XElement(ns("ExtensionId"), d.Name),
                    new XElement(ns("LoaderName"), d.LoaderName),
                    new XElement(ns("VirtualPath"), d.VirtualPath),
                    new XElement(ns("FileHash"), fileHashProvider(d.Name)),
                    new XElement(ns("References"), FilterReferences(d.References)
                        .Select(r => new XElement(ns("Reference"),
                        new XElement(ns("ReferenceId"), r.Name),
                        new XElement(ns("LoaderName"), r.LoaderName),
                        new XElement(ns("VirtualPath"), r.VirtualPath),
                        new XElement(ns("FileHash"), fileHashProvider(r.Name)))).ToArray())));

            document.Root.Add(elements);
            return document;
        }

        private IEnumerable<ActivatedExtensionDescriptor> ReadDescriptors(string persistancePath) {
            Func<string, XName> ns = (name => XName.Get(name));
            Func<XElement, string, string> elem = (e, name) => e.Element(ns(name)).Value;

            XDocument document = ReadDocument(persistancePath);
            return document
                .Elements(ns("Dependencies"))
                .Elements(ns("Dependency"))
                .Select(e => new ActivatedExtensionDescriptor {
                    ExtensionId = elem(e, "ExtensionId"),
                    VirtualPath = elem(e, "VirtualPath"),
                    LoaderName = elem(e, "LoaderName"),
                    FileHash = elem(e, "FileHash"),
                    //References = e.Elements(ns("References")).Elements(ns("Reference")).Select(r => new DependencyReferenceDescriptor {
                    //    Name = elem(r, "Name"),
                    //    LoaderName = elem(r, "LoaderName"),
                    //    VirtualPath = elem(r, "VirtualPath")
                    //})
                }).ToList();
        }

        private IEnumerable<DependencyDescriptor> FilterDependencies(IEnumerable<DependencyDescriptor> dependencies) {
            return dependencies.Where(dep => IsSupportedLoader(dep.LoaderName));
        }

        private IEnumerable<DependencyReferenceDescriptor> FilterReferences(IEnumerable<DependencyReferenceDescriptor> references) {
            return references.Where(dep => IsSupportedLoader(dep.LoaderName));
        }

        private bool IsSupportedLoader(string loaderName) {
            //Note: this is hard-coded for now, to avoid adding more responsibilities to the IExtensionLoader
            //      implementations.
            return
                loaderName == "DynamicExtensionLoader" ||
                loaderName == "PrecompiledExtensionLoader";
        }

        private void WriteDocument(string persistancePath, XDocument document) {
            _writeThroughToken.IsCurrent = false;
            using (var stream = _appDataFolder.CreateFile(persistancePath)) {
                document.Save(stream, SaveOptions.None);
                stream.Close();
            }
        }

        private XDocument ReadDocument(string persistancePath) {
            if (!_appDataFolder.FileExists(persistancePath))
                return new XDocument();

            try {
                using (var stream = _appDataFolder.OpenFile(persistancePath)) {
                    return XDocument.Load(stream);
                }
            }
            catch (Exception e) {
                Logger.Information(e, "Error reading file '{0}'", persistancePath);
                return new XDocument();
            }
        }

        private bool CompareXmlDocuments(XDocument doc1, XDocument doc2) {
            return XNode.DeepEquals(doc1.Root, doc2.Root);
        }

        private class InvalidationToken : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
    }
}