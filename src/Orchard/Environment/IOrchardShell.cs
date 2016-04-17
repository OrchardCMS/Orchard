using Orchard.Tasks;

namespace Orchard.Environment {
    public interface IOrchardShell {
        void Activate();
        void Terminate();
        ISweepGenerator Sweep { get; }
    }
}