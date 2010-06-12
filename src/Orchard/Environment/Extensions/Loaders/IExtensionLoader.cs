using System;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Loaders {
    public interface IExtensionLoader {
        int Order { get; }
        ExtensionProbeEntry Probe(ExtensionDescriptor descriptor);
        ExtensionEntry Load(ExtensionProbeEntry descriptor);
    }

    public class ExtensionProbeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public IExtensionLoader Loader { get; set; }
        public string VirtualPath { get; set; }
        public DateTime LastModificationTimeUtc { get; set; }
    }
}