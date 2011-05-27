using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;

namespace Orchard.FileSystems.Dependencies {
    /// <summary>
    /// Similar to "Dependencies.xml" file, except we also store "GetFileHash" result for every 
    /// VirtualPath entry. This is so that if any virtual path reference in the file changes,
    /// the file stored by this component will also change.
    /// </summary>
    public class DefaultModuleDependenciesManager : IModuleDependenciesManager {
        private readonly IAppDataFolder _appDataFolder;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private const string BasePath = "Dependencies";
        private const string FileName = "Dependencies.ModuleCompilation.xml";

        public DefaultModuleDependenciesManager(IAppDataFolder appDataFolder, IVirtualPathProvider virtualPathProvider) {
            _appDataFolder = appDataFolder;
            _virtualPathProvider = virtualPathProvider;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private string PersistencePath {
            get { return _appDataFolder.Combine(BasePath, FileName); }
        }

        public void StoreDependencies(IEnumerable<DependencyDescriptor> dependencyDescriptors) {
            Logger.Information("Storing module dependency file.");

            var newDocument = CreateDocument(dependencyDescriptors);
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

        private XDocument CreateDocument(IEnumerable<DependencyDescriptor> dependencies) {
            Func<string, XName> ns = (name => XName.Get(name));

            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = FilterDependencies(dependencies).Select(
                d => new XElement("Dependency",
                    new XElement(ns("ModuleName"), d.Name),
                    new XElement(ns("LoaderName"), d.LoaderName),
                    new XElement(ns("VirtualPath"), d.VirtualPath),
                    new XElement(ns("FileHash"), _virtualPathProvider.GetFileHash(d.VirtualPath)),
                    new XElement(ns("References"), FilterReferences(d.References)
                        .Select(r => new XElement(ns("Reference"),
                        new XElement(ns("Name"), r.Name),
                        new XElement(ns("LoaderName"), r.LoaderName),
                        new XElement(ns("VirtualPath"), r.VirtualPath),
                        new XElement(ns("FileHash"), _virtualPathProvider.GetFileHash(r.VirtualPath)))).ToArray())));

            document.Root.Add(elements);
            return document;
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
            catch(Exception e) {
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