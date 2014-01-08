namespace Orchard.Messaging.Services {
    public interface IMessageQueueProcessor : ISingletonDependency {
        void ProcessQueue();
    }
}