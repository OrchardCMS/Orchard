namespace Orchard.Messaging.Services {
    public interface IMessageService : IDependency {
        void Send(string type, string payload);
    }
}