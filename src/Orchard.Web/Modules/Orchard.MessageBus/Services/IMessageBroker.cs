using System;

namespace Orchard.MessageBus.Services
{
    public interface IMessageBroker : ISingletonDependency
    {
        void Subscribe(string channel, Action<string, string> handler);
        void Publish(string channel, string message);
    }
}
