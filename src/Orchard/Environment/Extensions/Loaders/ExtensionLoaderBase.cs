using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public abstract class ExtensionLoaderBase : IExtensionLoader {
        private readonly IDependenciesFolder _dependenciesFolder;

        protected ExtensionLoaderBase(IDependenciesFolder dependenciesFolder) {
            _dependenciesFolder = dependenciesFolder;
        }

        public abstract int Order { get; }
        public string Name { get { return this.GetType().Name; } }

        public abstract ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var dependency = _dependenciesFolder.GetDescriptor(descriptor.Name);
            if (dependency != null && dependency.LoaderName == this.Name) {
                return LoadWorker(descriptor);
            }
            return null;
        }

        public virtual void ExtensionActivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) { }
        public virtual void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension) { }
        public virtual void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) { }
        public virtual void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor) { }

        public abstract ExtensionEntry LoadWorker(ExtensionDescriptor descriptor);

        public virtual string GetWebFormAssemblyDirective(DependencyDescriptor dependency) {
            return null;
        }

        public virtual IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency) {
            return Enumerable.Empty<string>();
        }

        protected static bool IsAssemblyLoaded(string moduleName) {
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == moduleName);
        }
    }
}