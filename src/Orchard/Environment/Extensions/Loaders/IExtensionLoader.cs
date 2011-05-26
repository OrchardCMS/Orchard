using System;
using System.Collections.Generic;
using System.Reflection;
using Orchard.Caching;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Environment.Extensions.Loaders {
    public class ExtensionProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string VirtualPath { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
    }

    public class ExtensionReferenceProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string Name { get; set; }
        public string VirtualPath { get; set; }
    }

    public interface IExtensionLoader {
        int Order { get; }
        string Name { get; }

        IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor);
        Assembly LoadReference(DependencyReferenceDescriptor reference);
        void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry);
        bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references);

        ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);
        ExtensionEntry Load(ExtensionDescriptor descriptor);

        void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
        void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension);
        void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency);

        void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor);

        string GetWebFormAssemblyDirective(DependencyDescriptor dependency);
        IEnumerable<string> GetWebFormVirtualDependencies(DependencyDescriptor dependency);
    }
}