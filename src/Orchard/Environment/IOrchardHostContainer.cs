namespace Orchard.Environment {
    public interface IOrchardHostContainer {
        T Resolve<T>();
    }
}