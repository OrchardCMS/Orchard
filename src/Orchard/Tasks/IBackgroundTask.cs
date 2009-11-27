namespace Orchard.Tasks {
    public interface IBackgroundTask : IEvents {
        void Sweep();
    }
}
