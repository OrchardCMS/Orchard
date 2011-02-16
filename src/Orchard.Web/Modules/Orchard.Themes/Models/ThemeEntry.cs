using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.Models {
     public class ThemeEntry {
        public ThemeEntry() {}

        public ThemeEntry(ExtensionDescriptor extensionDescriptor) {
            Descriptor = extensionDescriptor;
        }

        public ExtensionDescriptor Descriptor { get; set; }

        public bool Enabled { get; set; }

        public bool NeedsUpdate { get; set; }

        public string Name { get { return Descriptor.Name; } }
    }
}
