using Orchard.Events;

namespace Orchard.Environment.Extensions {
    public interface IExtensionManagerEvents : IEventHandler {
        void ModuleChanged(string moduleName);
    }
}