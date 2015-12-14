namespace Orchard.Tasks {
    public interface IBackgroundTask : IDependency {
        void Sweep();
    }
}
