namespace Orchard.Environment {
    public interface IOrchardShellEvents : IEvents {
        void Activated();
        void Terminating();
    }
}
