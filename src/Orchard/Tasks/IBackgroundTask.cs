using Orchard.Events;

namespace Orchard.Tasks {
    public interface IBackgroundTask : IEventHandler {
        void Sweep();
    }
}
