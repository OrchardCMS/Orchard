using System.Collections.Generic;

namespace Orchard.Extensions {
    public interface IExtensionManagerEvents : IEvents {
        void Enabling(ExtensionEventContext context);
        void Enabled(ExtensionEventContext context);
        void Disabling(ExtensionEventContext context);
        void Disabled(ExtensionEventContext context);
    }

    public class ExtensionEventContext {
        public ExtensionEntry Extension { get; set; }
        public IEnumerable<ExtensionEntry> EnabledExtensions { get; set; }
    }
}
