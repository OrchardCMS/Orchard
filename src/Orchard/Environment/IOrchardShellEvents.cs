using Orchard.Environment.Extensions.Models;
using Orchard.Events;

namespace Orchard.Environment {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }

    public interface IFeatureEventHandler : IEventHandler {
        void Installing(Feature feature);
        void Installed(Feature feature);
        void Enabling(Feature feature);
        void Enabled(Feature feature);
        void Disabling(Feature feature);
        void Disabled(Feature feature);
        void Uninstalling(Feature feature);
        void Uninstalled(Feature feature);
    }
}
