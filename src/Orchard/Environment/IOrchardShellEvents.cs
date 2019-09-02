using Orchard.Events;

namespace Orchard.Environment {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
