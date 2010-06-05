using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.Environment;

namespace Orchard.FileSystems.Dependencies {
    public interface IDependenciesFolder : IVolatileProvider {
        void StoreAssemblyFile(string assemblyName, string assemblyFileName);
        Assembly LoadAssembly(string assemblyName);
    }

    public class DefaultDependenciesFolder : IDependenciesFolder {
        private const string _basePath = "~/App_Data/Dependencies";
        private readonly IVirtualPathProvider _virtualPathProvider;

        public DefaultDependenciesFolder(IVirtualPathProvider virtualPathProvider) {
            _virtualPathProvider = virtualPathProvider;
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

        public void StoreAssemblyFile(string assemblyName, string assemblyFileName) {
            StoreAssemblyFile(assemblyName, assemblyFileName, Path.GetFileName(assemblyFileName));
        }

        private void StoreAssemblyFile(string assemblyName, string assemblyFileName, string destinationFileName) {
            _virtualPathProvider.CreateDirectory(BasePath);

            var destinationPath = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(BasePath, destinationFileName));
            File.Copy(assemblyFileName, destinationPath);

            StoreDepencyInformation(assemblyName, destinationFileName);
        }

        private void StoreDepencyInformation(string name, string fileName) {
            var dependencies = ReadDependencies().ToList();

            var dependency = dependencies.SingleOrDefault(d => d.Name == name);
            if (dependency == null) {
                dependency = new DependencyDescritpor { Name = name, FileName = fileName };
                dependencies.Add(dependency);
            }
            dependency.FileName = fileName;

            WriteDependencies(dependencies);
        }

        public Assembly LoadAssembly(string assemblyName) {
            _virtualPathProvider.CreateDirectory(BasePath);

            var dependency = ReadDependencies().SingleOrDefault(d => d.Name == assemblyName);
            if (dependency == null)
                return null;

            if (!_virtualPathProvider.FileExists(_virtualPathProvider.Combine(BasePath, dependency.FileName)))
                return null;

            return Assembly.Load(Path.GetFileNameWithoutExtension(dependency.FileName));
        }

        private class DependencyDescritpor {
            public string Name { get; set; }
            public string FileName { get; set; }
        }

        private IEnumerable<DependencyDescritpor> ReadDependencies() {
            if (!_virtualPathProvider.FileExists(PersistencePath))
                return Enumerable.Empty<DependencyDescritpor>();

            using (var stream = _virtualPathProvider.OpenFile(PersistencePath)) {
                XDocument document = XDocument.Load(stream);
                return document
                    .Elements(ns("Dependencies"))
                    .Elements(ns("Dependency"))
                    .Select(e => new DependencyDescritpor { Name = e.Element("Name").Value, FileName = e.Element("FileName").Value })
                    .ToList();
            }
        }

        private void WriteDependencies(IEnumerable<DependencyDescritpor> dependencies) {
            var document = new XDocument();
            document.Add(new XElement(ns("Dependencies")));
            var elements = dependencies.Select(d => new XElement("Dependency",
                                                  new XElement(ns("Name"), d.Name),
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
