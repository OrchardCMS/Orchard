using Orchard.Environment.Extensions.Models;
using Orchard.Events;

namespace Orchard.Environment {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }

    public interface IFeatureEventHandler : IEventHandler {
        void Install(Feature feature);
        void Enable(Feature feature);
        void Disable(Feature feature);
        void Uninstall(Feature feature);
    }
}
