using System;
using System.Collections.Generic;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public class ExtensionProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string VirtualPath { get; set; }
        public DateTime LastModificationTimeUtc { get; set; }
    }

    public interface IExtensionLoader {
        int Order { get; }
        string Name { get; }

        ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);
        ExtensionEntry Load(ExtensionDescriptor descriptor);

        void ExtensionActivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension);
        void ExtensionDeactivated(ExtensionLoadingContext ctx, bool isNewExtension, ExtensionDescriptor extension);
        void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency);

        void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor);

        string GetWebFormAssemblyDirective(DependencyDescriptor dependency);
        IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency);
    }
}