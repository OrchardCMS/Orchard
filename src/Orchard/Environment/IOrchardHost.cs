namespace Orchard.Environment {
    public interface IOrchardHost {
        void Initialize();
        void EndRequest();
        IOrchardRuntime CreateRuntime();
    }
}