using System.Collections.Generic;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManagerEvents : IEvents {
        void Enabling(ExtensionEventContext context);
        void Enabled(ExtensionEventContext context);
        void Disabling(ExtensionEventContext context);
        void Disabled(ExtensionEventContext context);
        void Activating(ExtensionEventContext context);
        void Activated(ExtensionEventContext context);
        void Deactivating(ExtensionEventContext context);
        void Deactivated(ExtensionEventContext context);
    }

    public abstract class ExtensionManagerEvents : IExtensionManagerEvents {
        public virtual void Enabling(ExtensionEventContext context) { }
        public virtual void Enabled(ExtensionEventContext context) { }
        public virtual void Disabling(ExtensionEventContext context) {}
        public virtual void Disabled(ExtensionEventContext context) {}
        public virtual void Activating(ExtensionEventContext context) {}
        public virtual void Activated(ExtensionEventContext context) {}
        public virtual void Deactivating(ExtensionEventContext context) {}
        public virtual void Deactivated(ExtensionEventContext context) {}
    }

    public class ExtensionEventContext {
        public ExtensionEntry Extension { get; set; }
        public IEnumerable<ExtensionEntry> EnabledExtensions { get; set; }
    }
}