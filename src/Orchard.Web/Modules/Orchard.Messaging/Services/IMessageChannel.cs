namespace Orchard.Messaging.Services {
    public interface IMessageChannel : IDependency {
        void Process(string payload);
    }
}